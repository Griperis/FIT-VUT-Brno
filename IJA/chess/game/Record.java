package chess.game;

import java.util.ArrayList;

/**
 * @author Marek Nesvadba, Zdeněk Doležal (xnesva06, xdolez82)
 * <p>List of halfmoves, which has replaying functions to step the record;
 */

public class Record {
    private ArrayList<Move> moves;
    private int moveArrayIndex;
    private int maxIndex;

    public Record() {
        this.moves = new ArrayList<>();
        this.moveArrayIndex = 0;
        this.maxIndex = -1;
    }

    public void addMove(Move move) {
        if (moveArrayIndex < moves.size()) {
            moves.set(moveArrayIndex++, move);
            System.out.println("Index: " + moveArrayIndex + " Size: " + getSize() + " MI: " + maxIndex);
            for (int i = moveArrayIndex; moveArrayIndex != moves.size(); ) {
                System.out.println("removing " + moves.get(i).toString());
                moves.remove(i);
            }
        } else {
            moves.add(moveArrayIndex++, move);
        }
    }

    public Move getNextMove() {
        if (moveArrayIndex < getSize()) {
            return moves.get(moveArrayIndex++);
        } else {
            return null;
        }
    }

    public Move getPrevMove() {
        if (moveArrayIndex - 1 >= 0) {
            return moves.get(--moveArrayIndex);
        } else {
            return null;
        }
    }

    public Record getValidPart() {
        Record record = new Record();
        for (int i = 0; i < getSize(); i++) {
            if (maxIndex != -1 && maxIndex == i) break;
            record.addMove(new Move(moves.get(i)));
        }
        return record;
    }

    public int getIndex() {
        return this.moveArrayIndex;
    }

    public void setMaxIndex(int val) {
        if (val < moves.size()) {
            this.maxIndex = val;
        } else {
        }
    }

    public void resetMaxIndex() {
        this.maxIndex = -1;
    }

    public void resetIndex() {
        this.moveArrayIndex = 0;
    }

    public int getSize() {
        int size = moves.size();
        if (maxIndex != -1) {
            size = maxIndex + 1;
        }
        return size;
    }

    public static void append(Record record, Record append) {
        for (Move m : append.moves) {
            record.addMove(m);
        }
    }

    public String[] toStringArray() {
        int sizeModifier = 0;
        if (moves.size() % 2 == 1) {
            sizeModifier = 1;
        }
        String[] result = new String[moves.size() / 2 + sizeModifier];
        int arrIndex = 0;
        for (int i = 0; i < moves.size(); i += 2) {
            Move white = moves.get(i);
            Move black = null;
            if (i + 1 < moves.size()) {
                black = moves.get(i + 1);
            }
            result[arrIndex] = (arrIndex + 1) + ". " + white.toString() + " " + (black != null ? black.toString() : "");
            if (i + 1 < moves.size()) {
                arrIndex++;
            }

        }
        return result;
    }

    public void clear() {
        moves = new ArrayList<>();
        this.moveArrayIndex = 0;
        this.maxIndex = -1;
    }
}
