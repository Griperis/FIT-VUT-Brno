/*
IPK Project 2 Header File 
TCP/UDP port scanner
Author: Zdenek Dolezal (xdolez82)
Resources not mentioned here detaily are mentioned in documentation
*/

#ifndef SCAN
#define SCAN

#include <getopt.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <sys/types.h>
#include <pcap/pcap.h>
#include <pcap.h>
#include <netdb.h>
#include <signal.h>
#include <unistd.h>
#include <netinet/ip.h>
#include <netinet/tcp.h>
#include <netinet/udp.h>
#include <netinet/ether.h>

#include <iostream>
#include <vector>
#include <regex>
#include <string>

// Internal constants
#define TCP_TIMEOUT 1
#define UDP_TIMEOUT 1
#define UDP_PACKET_BATCH_SIZE 4

#define EXIT_OK 0
#define EXIT_ERROR_INTERNAL 1
#define EXIT_ERROR_PARAM 2
#define EXIT_ERROR_PCAP 3

// TCP pseudoheader
struct p_tcphdr
{
    u_int32_t ip_saddr;
    u_int32_t ip_daddr;
    u_int8_t zero;
    u_int8_t proto;
    u_int16_t tcp_len;
};

/*
Checks if port is of expected value
@param port - value to check
@return bool - true if ok / false otherwise
*/
bool is_valid_port(int port);

/*
Check if port interval is specified correctly
@param low - lower endpoint
@param high - higher endpoint
@return bool - true if ok / false otherwise
*/
bool is_valid_port_range(int low, int high);

/*
Parses port range passed from argument into vector of int as ports
@param s - string to parse
@return vector<int> - vector of ports as int
*/
std::vector<int> get_ports(std::string s);

/*
Checksum implementation
Source: https://www.tenouk.com/Module43a.html
        https://github.com/rbaron/raw_tcp_socket/blob/master/raw_tcp_socket.c

@param ptr - pointer to packet
@param nbytes - packet size
@return - calculated checksum
*/
u_int16_t check_sum(u_int16_t *ptr, unsigned int nbytes);

/*
Fills ipv4 header with common data and different data specified by parameters
@param ip_h - ip header to fill
@param len - total length
@param protocol - protocol to fill into ipheader
@param dev - device to get source address from
@param dst - destination address
*/
void fill_ipv4_header(iphdr *ip_h, int len, u_int8_t protocol, pcap_if_t *dev, sockaddr_in *dst);

/*
Abstraction for socket creation and setting socket options

@param inet - ipv4 or ipv6
@param protocol - tcp or udp
@return - socket descriptor on success / -1 otherwise
*/
int create_socket(int inet, int protocol);

/*
Runs TCP scan on dst address ports specified by param ports using device dev
@param src_port - source port
@param ports - ports to scan
@param dst - destination address
@param dev - device to be used

@return - 0 on sucess / EXIT_ERROR_INTERNAl or EXIT_ERROR_PCAP on failure
*/
int TCP_scan(int src_port, std::vector<int> ports, sockaddr_in *dst, pcap_if_t *dev);

/* 
Handler for timeout packets
Warning: sets was_timeout global variable (for resending of tcp packets)
@param signum - signal number
*/
void TCP_timeout_handler(int signum);

/* 
Pcap TCP packet callback handler
(tcp packet coming from destination to our device)
Parameters specified by pcap documentation
*/
void TCP_handler(u_char *args, const struct pcap_pkthdr *header, const u_char *packet);

/* 
Creates filter and compiles pcap for scanning of one port
Source: https://www.tcpdump.org/pcap.html (others in documentation)

@param port - port to build pcap for
@param dst_addr_str - destination address to filter packets from
@param src_port - port where packet was send from
@param src_addr_str - source address to filter packets incoming to
@param dev - used device

@return - 0 on success / EXIT_ERROR_PCAP on failure
*/
int build_pcap_TCP(int port, char* dst_addr_str, int src_port, char* src_addr_str, pcap_if_t *dev);

/*
Runs UDP scan on dst address ports specified by param ports using device dev
Sends from 1 to UDP_PACKET_BATCH_SIZE of packets to dst
Warning: uses global variable got_answer to determine if icmp packet came back

@param src_port - source port
@param ports - ports to scan
@param dst - destination address
@param dev - device to be used

@return - 0 on sucess / EXIT_ERROR_INTERNAl or EXIT_ERROR_PCAP on failure
*/
int UDP_scan(int src_port, std::vector<int> ports, sockaddr_in *dst, pcap_if_t *dev);

/*
Pcap UDP packet callback handler - called when filtered papcket captured
(icmp packet with message type 3 and message code 3 for UDP)
Warning: sets global variable got_answer
Parameters specified by pcap documentation
*/
void UDP_handler(u_char *args, const struct pcap_pkthdr *header, const u_char *packet);

/*
Only for stopping pcap loop
*/
void UDP_timeout_handler(int signum);

#endif