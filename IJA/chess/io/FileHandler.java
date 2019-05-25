package chess.io;

import chess.game.Record;

import java.io.*;

/**
 * @author Marek Nesvadba, Zdeněk Doležal (xnesva06, xdolez82)
 * <p>Class responsible for loading and saving files.
 */

public class FileHandler {
    /**
     * Loads file to loadedRecord
     * @param loadedRecord
     * @param file
     */
    public boolean loadRecord(File file, Record loadedRecord) {
        try {
            BufferedReader br = new BufferedReader(new FileReader(file));

            String line;
            Parser parser = new Parser();
            while ((line = br.readLine()) != null) {
                if (parser.parseLine(loadedRecord, line) == false) {
                    loadedRecord.clear();
                    return false;
                }
            }
        } catch (Exception e) {
            System.out.println(e.getMessage());
            loadedRecord.clear();
            return false;
        }
        return true;
    }

    /**
     * Saves record to file
     * @param record
     * @param file
     */
    public boolean saveRecord(Record record, File file) {
        try {
            String[] recordSplit = record.toStringArray();
            FileWriter fileWriter = new FileWriter(file);
            for (String s : recordSplit) {
                fileWriter.write(s + "\n");
            }
            fileWriter.close();
        } catch (IOException e) {
            System.out.println(e.getMessage());
            return false;
        }
        return true;
    }
}
