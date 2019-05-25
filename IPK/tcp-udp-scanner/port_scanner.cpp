/*
IPK Project 2 Implementation
TCP/UDP port scanner
Author: Zdenek Dolezal (xdolez82)
*/

#include "port_scanner.h"

using namespace std;

pcap_t *handle;    // Handle for packet scanning
int was_timeout;   // Controls sending of second TCP packet when scanning  
bool got_answer;   // If true UDP scanning got answer so port is closed

bool is_valid_port(int port)
{
    return port > 0 && port < 65536;
}

bool is_valid_port_range(int low, int high)
{
    return low <= high && is_valid_port(low) && is_valid_port(high);
}

vector<int> get_ports(string s)
{
    vector<int> ports;

    smatch match;
    regex regex_range("^(\\d+)-(\\d+)$");
    regex regex_sequence("^(\\d+,?)+$");
    regex regex_sequence_part("((\\d+),?)+?");
    if (regex_search(s, match, regex_range))
    {
        // Range 1 - 65536
        if (match.size() != 3)
        {
            cerr << "Error: Invalid port range argument" << endl;
            exit(EXIT_ERROR_PARAM);
        }

        int low = stoi(match.str(1));
        int high = stoi(match.str(2));
        if (is_valid_port_range(low, high))
        {
            for (int i = low; i <= high; ++i)
            {
                ports.push_back(i);
            }
            return ports;
        }
        else
        {
            cerr << "Error: Invalid port range in argument" << endl;
            exit(EXIT_ERROR_PARAM);
        }
    }
    else if (regex_search(s, match, regex_sequence))
    {
        //Sequence
        for (sregex_iterator i = sregex_iterator(s.begin(), s.end(), regex_sequence_part); i != sregex_iterator(); ++i)
        {
            smatch m = *i;
            int port = stoi(m.str(2));
            if (!is_valid_port(port))
            {
                cerr << "Error: Invalid port range in argument " << endl;
                exit(EXIT_ERROR_PARAM);
            }
            ports.push_back(stoi(m.str(2)));
        }
        return ports;
    }
    else
    {
        cerr << "Error: Unsupported format of ports" << endl;
        exit(EXIT_ERROR_PARAM);
    }

    return ports;
}

void find_first_nonlbdev(pcap_if_t *devices, pcap_if_t **dev, string name)
{
    for (*dev = devices; *dev != NULL; *dev = (*dev)->next)
    {
        if (name != "" && name == (*dev)->name)
        {
            return;
        }
        else if (name == "" && (*dev)->flags != PCAP_IF_LOOPBACK)
        {
            return;
        }
    }
    cerr << "Error: No device " << name << " was found" << endl;
    pcap_freealldevs(devices);
    exit(EXIT_ERROR_INTERNAL);
}

sockaddr_in *get_dev_addr(pcap_if_t *dev)
{
    for (pcap_addr_t *addr = dev->addresses; addr != NULL; addr = addr->next)
    {
        if (addr->addr->sa_family == AF_INET)
        {
            return (sockaddr_in *)(addr->addr);
        }
    }
    cerr << "Error: Device has no INET address" << endl;
    exit(EXIT_ERROR_INTERNAL);
}

u_int16_t check_sum(u_int16_t *ptr,unsigned int nbytes) 
{
	u_int32_t sum = 0;
    for (; nbytes > 1; nbytes -= 2){
        sum += *ptr++;
    }
    // oddbyte 
    if (nbytes == 1)
    {
        sum += *(u_char*)ptr;
    }

	sum = (sum>>16)+(sum & 0xffff);
	sum += (sum>>16);
	
	return (u_int16_t)~sum;
}

void fill_ipv4_header(iphdr *ip_h, int len, u_int8_t protocol, pcap_if_t *dev, sockaddr_in *dst)
{
    ip_h->version = 4;
    ip_h->ihl = 5;
    ip_h->tos = 0;
    ip_h->tot_len = htons(len); 
    ip_h->id = htonl(54321);
    ip_h->ttl = 255;
    ip_h->protocol = protocol;
    ip_h->saddr = get_dev_addr(dev)->sin_addr.s_addr;
    ip_h->daddr = dst->sin_addr.s_addr;
    ip_h->check = 0;
}

int create_socket(int inet, int protocol)
{
    int sck = socket(inet, SOCK_RAW, protocol);
    if (sck == -1)
    {
        return -1;
    }
    int one;
    if (setsockopt(sck, IPPROTO_IP, IP_HDRINCL, &one, sizeof(one)) == -1)
    {
        cerr << "Error: could not set socket options" << endl;
        return -1;
    }
    return sck;
}

int build_pcap_TCP(int port, char* dst_addr_str, int src_port, char* src_addr_str, pcap_if_t *dev)
{
    char pcap_err[PCAP_ERRBUF_SIZE];
    stringstream filter_exp;
    filter_exp << "tcp && src port " << port << " && src host " << dst_addr_str 
                << " && dst port " << src_port << " && dst host " << src_addr_str;
    struct bpf_program fp;
    bpf_u_int32 mask;
    bpf_u_int32 net;
    
    if (pcap_lookupnet(dev->name, &net, &mask, pcap_err) == -1)
    {
        cerr << "Error: couldn not get netmask for dev " << dev->name << endl;
        net = 0;
        mask = 0; 
    }
    // TODO BUFSIZ / false idk
    handle = pcap_open_live(dev->name, BUFSIZ, false, 1000, pcap_err);
    if (handle == NULL)
    {
        cerr << "Error: could not create pcap sniffer for dev " << dev->name << endl;
        return EXIT_ERROR_PCAP;
    }
    if (pcap_compile(handle, &fp, filter_exp.str().c_str(), 0, net) == -1)
    {
        cerr << "Error: could not compile filter: " << pcap_geterr(handle) << endl;
        return EXIT_ERROR_PCAP;
    }
    if (pcap_setfilter(handle, &fp) == -1)
    {
        cerr << "Error: could not set filter: " << pcap_geterr(handle) << endl;
        return EXIT_ERROR_PCAP;
    }
    return 0;
}

int TCP_scan(int src_port, vector<int> ports, sockaddr_in *dst, pcap_if_t *dev)
{
    // Adresses in readable format
    char src_addr_str[INET_ADDRSTRLEN];
    char dst_addr_str[INET_ADDRSTRLEN];
    inet_ntop(AF_INET, &dst->sin_addr, dst_addr_str, INET_ADDRSTRLEN);
    inet_ntop(AF_INET, &(get_dev_addr(dev)->sin_addr), src_addr_str, INET_ADDRSTRLEN);

    // create socket
    int sck = create_socket(PF_INET, IPPROTO_TCP);
    if (sck == -1)
    {
        cerr << "Error: socket creation failed" << endl;
        return EXIT_ERROR_INTERNAL;
    }

    // Create packet structure [IP HEADER + TCP HEADER + PSEUDO HEADER]
    int tcp_packet_size = sizeof(iphdr) + sizeof(tcphdr) + sizeof(p_tcphdr);
    char *packet = (char*)malloc(tcp_packet_size);
    if (packet == NULL)
    {
        cerr << "Error: memory allocation failed" << endl;
        return EXIT_ERROR_INTERNAL;
    }
    memset(packet, 0, tcp_packet_size);
    // IP header is the same everytime
    iphdr *ip_h = (iphdr*) packet;
    tcphdr *tcp_h = (tcphdr*) (packet + sizeof(iphdr));
    fill_ipv4_header(ip_h, IPPROTO_TCP, sizeof(iphdr) + sizeof(tcphdr), dev, dst);

    // send packet to ports
    for (vector<int>::iterator it = ports.begin(); it != ports.end(); it++)
    {
        was_timeout = 0;
        // filling tcpheader
        tcp_h->source = htons(src_port);
        tcp_h->dest = htons(*it);
        tcp_h->seq = 0;
        tcp_h->th_flags = TH_SYN;
        tcp_h->doff = 5;
        tcp_h->syn = 1;
        tcp_h->ack_seq = 0;
        tcp_h->check = 0;

        // filling pseudoheader
        p_tcphdr *pseudo_h = (p_tcphdr*)(packet + sizeof(iphdr) + sizeof(tcphdr));
        pseudo_h->ip_saddr = get_dev_addr(dev)->sin_addr.s_addr;
        pseudo_h->ip_daddr = dst->sin_addr.s_addr;
        pseudo_h->zero = 0;
        pseudo_h->proto = IPPROTO_TCP;
        pseudo_h->tcp_len = ntohs(sizeof(tcphdr));

        // now we can calculate checksum
        tcp_h->check = check_sum((unsigned short*)tcp_h, sizeof(p_tcphdr) + sizeof(tcphdr));
        
        if (build_pcap_TCP(*it, dst_addr_str, src_port, src_addr_str, dev) == EXIT_ERROR_PCAP)
        {
            free(packet);
            return EXIT_ERROR_PCAP;
        }

        int send = sendto(sck, packet, ip_h->tot_len, 0, (sockaddr*)dst, sizeof(sockaddr));
        if (send < 0)
        {
            cerr << "Error: could not send TCP packet, errno: " << errno << endl;
            free(packet);
            return EXIT_ERROR_INTERNAL;
        }
        cout << *it << "/tcp: ";

        alarm(TCP_TIMEOUT);
        signal(SIGALRM, TCP_timeout_handler);

        pcap_loop(handle, 0, TCP_handler, NULL);

        //if no response from first packet send next one
        if (was_timeout == 1)
        {
            int send = sendto(sck, packet, ip_h->tot_len, 0, (sockaddr*)dst, sizeof(sockaddr));
            if (send < 0){
                cerr << "Error: could not send TCP packet" << endl;
                free(packet);
                return EXIT_ERROR_INTERNAL;
            }
            alarm(TCP_TIMEOUT);
            signal(SIGALRM, TCP_timeout_handler);

            pcap_loop(handle, 0, TCP_handler, NULL);
        }

        pcap_close(handle);
    }
    free(packet);
    return 0;

}

void TCP_timeout_handler(int signum)
{
    if (was_timeout)
    {
        cout << "Filtered" << endl;
    }
    else
    {
        was_timeout = 1;        
    }
    pcap_breakloop(handle);
}

void TCP_handler(u_char *args, const struct pcap_pkthdr *header, const u_char *packet)
{
    iphdr *ip_h = (iphdr*)(packet + sizeof(ethhdr));
    tcphdr *tcp_h = (tcphdr*)(packet + sizeof(ethhdr) + ip_h->ihl*4); 
    if (tcp_h->ack == 1 && tcp_h->rst == 1)
    {
        cout << "Closed" << endl;
    }
    else if (tcp_h->ack == 1)
    {
        cout << "Open" << endl;
    }
    pcap_breakloop(handle);
}

int build_pcap_UDP(int port, char* dst_addr_str, int src_port, char* src_addr_str, pcap_if_t *dev)
{
    char pcap_err[PCAP_ERRBUF_SIZE];
    stringstream filter_exp;
    filter_exp << "icmp && icmp[icmptype] = 3 && icmp[icmpcode] = 3 && src host " << dst_addr_str << " && dst host " << src_addr_str;
    struct bpf_program fp;
    bpf_u_int32 mask;
    bpf_u_int32 net;
    
    if (pcap_lookupnet(dev->name, &net, &mask, pcap_err) == -1)
    {
        cerr << "Error: couldn not get netmask for dev " << dev->name << endl;
        net = 0;
        mask = 0; 
    }
    // TODO BUFSIZ / false idk
    handle = pcap_open_live(dev->name, BUFSIZ, false, 1000, pcap_err);
    if (handle == NULL)
    {
        cerr << "Error: could not create pcap sniffer for dev " << dev->name << endl;
        return EXIT_ERROR_PCAP;
    }
    if (pcap_compile(handle, &fp, filter_exp.str().c_str(), 0, net) == -1)
    {
        cerr << "Error: could not compile filter: " << pcap_geterr(handle) << endl;
        return EXIT_ERROR_PCAP;
    }
    if (pcap_setfilter(handle, &fp) == -1)
    {
        cerr << "Error: could not set filter: " << pcap_geterr(handle) << endl;
        return EXIT_ERROR_PCAP;
    }
    return 0;
}

int UDP_scan(int src_port, std::vector<int> ports, sockaddr_in *dst, pcap_if_t *dev)
{
    char src_addr_str[INET_ADDRSTRLEN];
    char dst_addr_str[INET_ADDRSTRLEN];
    inet_ntop(AF_INET, &dst->sin_addr, dst_addr_str, INET_ADDRSTRLEN);
    inet_ntop(AF_INET, &(get_dev_addr(dev)->sin_addr), src_addr_str, INET_ADDRSTRLEN);

    int sck = create_socket(PF_INET, IPPROTO_UDP);
    if (sck == -1)
    {
        cerr << "Error: socket creation failed" << endl;
        return EXIT_ERROR_INTERNAL;
    }
    int udp_packet_size = sizeof(iphdr) + sizeof(udphdr);
    char *packet = (char*)malloc(udp_packet_size);
    if (packet == NULL)
    {
        cerr << "Error: memory allocation failed" << endl;
        return EXIT_ERROR_INTERNAL;
    }
    memset(packet, 0, udp_packet_size);
    iphdr *ip_h = (iphdr*)packet;
    udphdr *udp_h = (udphdr*)(packet + sizeof(iphdr));
    fill_ipv4_header(ip_h, IPPROTO_UDP, sizeof(iphdr) + sizeof(udphdr), dev, dst);

    for (vector<int>::iterator it = ports.begin(); it != ports.end(); it++)
    {
        udp_h->source = htons(src_port);
        udp_h->dest = htons(*it);
        udp_h->len = htons(sizeof(udphdr));
        udp_h->check = 0;

        if (build_pcap_UDP(*it, dst_addr_str, src_port, src_addr_str, dev) == EXIT_ERROR_PCAP)
        {
            free(packet);
            return EXIT_ERROR_PCAP;
        }

        cout << *it << "/udp: ";
        for (int i = 0; i <= UDP_PACKET_BATCH_SIZE && !got_answer; i++){
            int send = sendto(sck, packet, ip_h->tot_len, 0, (sockaddr*)dst, sizeof(sockaddr));
            if (send < 0)
            {
                cerr << "Error: could not send UDP packet, errno: " << errno << endl;
                free(packet);
                return EXIT_ERROR_INTERNAL;
            }
            alarm(UDP_TIMEOUT);
            signal(SIGALRM, UDP_timeout_handler);
            pcap_loop(handle, 1, UDP_handler, NULL);
        }
        if (got_answer)
        {
            cout << "Closed" << endl;
        }
        else
        {
            cout << "Open" << endl;
        }
        pcap_close(handle);
    }
    free(packet);
    return 0;
}
void UDP_handler(u_char *args, const struct pcap_pkthdr *header, const u_char *packet)
{   
    got_answer = true;
    pcap_breakloop(handle);
}
void UDP_timeout_handler(int signum)
{
    pcap_breakloop(handle);
}

int main(int argc, char **argv)
{
    vector<int> tcp_ports;
    vector<int> udp_ports;  
    string ip_input;
    string interface_name = "";

    // Argument parsing 
    int getopt_ret;
    while (42)
    {
        static struct option long_options[] = {
            {"pt", required_argument, 0, 't'},
            {"pu", required_argument, 0, 'u'},
            {"i", required_argument, 0, 'i'},
            {0, 0, 0, 0}};
        int option_index = 0;
        getopt_ret = getopt_long_only(argc, argv, "", long_options, &option_index);
        if (getopt_ret == -1)
            break;

        switch (getopt_ret)
        {
        case 't':
            tcp_ports = get_ports(optarg);
            break;

        case 'u':
            udp_ports = get_ports(optarg);
            break;
        case 'i':
            interface_name = optarg;
            break;
        case '?':
            exit(EXIT_ERROR_PARAM);

        default:
            cerr << "default" << endl;
            exit(EXIT_ERROR_PARAM);
        }
    }
    int max_arg_count = 6;
    if (!interface_name.empty())
    {
        max_arg_count = 8;
    }
    if (argc != max_arg_count)
    {
        cerr << "Error: Invalid number of arguments" << endl;
        return EXIT_ERROR_PARAM;
    }
    ip_input = argv[max_arg_count - 1];

    pcap_if_t *devices;
    pcap_if_t *dev = NULL;
    char errbuf[PCAP_ERRBUF_SIZE];
    if (pcap_findalldevs(&devices, errbuf) == PCAP_ERROR)
    {
        cerr << "Error: findalldevs error" << endl;
        return EXIT_ERROR_INTERNAL;
    }
    find_first_nonlbdev(devices, &dev, interface_name);

    /* IPv4 */
    //convert destination addr to ip format
    hostent *host = gethostbyname(ip_input.c_str());
    /*if(host->h_addrtype == AF_INET6)
    {
        cout << "DEBUG: IPv6" << endl;
    }
    else if (host->h_addrtype == AF_INET)
    {
        cout << "DEBUG: IPv4" << endl;
    }*/

    in_addr *dst = (in_addr*) *host->h_addr_list;
    sockaddr_in target_ip;
    target_ip.sin_family = AF_INET;
    target_ip.sin_port = 20;
    target_ip.sin_addr.s_addr = dst->s_addr;
    
    int src_port = 55443;
    char readable_target_ip[INET_ADDRSTRLEN];
    inet_ntop(AF_INET, &(target_ip.sin_addr), readable_target_ip, INET_ADDRSTRLEN);
    cout << "Scanning address: " << readable_target_ip << " (" << host->h_name << ")" << ", Using device: " << dev->name << endl;
    int ret_val = TCP_scan(src_port, tcp_ports, &target_ip, dev);
    if (ret_val != 0)
    {
        cerr << "Error in TCP_scan, ending..." << endl;
        pcap_freealldevs(devices);
        return ret_val;
    }
    ret_val = UDP_scan(src_port, udp_ports, &target_ip, dev);
    if (ret_val != 0)
    {
        cerr << "Error in UDP_scan, ending..." << endl;
        pcap_freealldevs(devices);
        return ret_val;
    }
    pcap_freealldevs(devices);
    return 0;
}
