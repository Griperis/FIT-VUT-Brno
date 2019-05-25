package chess.figures;

import chess.board.ChessBoard;
import chess.board.Field;

import java.util.ArrayList;

/**
 * @author Marek Nesvadba, Zdeněk Doležal (xnesva06, xdolez82)
 * <p> Class representing the pawn figure.
 */

public class Pawn extends Figure {

    public Pawn(int row, int column, boolean isOnBoard, FigureColor figureColor) {
        super(row, column, isOnBoard, figureColor, FigureType.Pawn);
    }

    public Pawn(Pawn pawn) {
        super(pawn);
    }

    @Override
    public ArrayList<Field> getPossibleMoveFields(ChessBoard board) {
        final int STARTING_ROW_WHITE = 1;
        final int STARTING_ROW_BLACK = 6;

        ArrayList<Field> possibleMoveFields = new ArrayList<>();
        if (figureColor == FigureColor.White) {
            Field upRight = board.getField(row + 1, column + 1);
            Field upLeft = board.getField(row + 1, column - 1);
            Field up = board.getField(row + 1, column);
            int canJump = checkNextRow(possibleMoveFields, upRight, upLeft, up);

            checkMoveFromStart(board, STARTING_ROW_WHITE, possibleMoveFields, up, canJump);
        } else {
            Field downRight = board.getField(row - 1, column + 1);
            Field downLeft = board.getField(row - 1, column - 1);
            Field down = board.getField(row - 1, column);
            int canJump = checkNextRow(possibleMoveFields, downRight, downLeft, down);

            checkMoveFromStart(board, STARTING_ROW_BLACK, possibleMoveFields, down, canJump);
        }
        return possibleMoveFields;
    }

    private void checkMoveFromStart(ChessBoard board, int sideStartingRow, ArrayList<Field> possibleMoveFields, Field middle, int canJump) {
        if (canJump == 0 && row == sideStartingRow) {
            int direction = sideStartingRow == 1 ? 2 : -2;
            Field startPlusTwo = board.getField(sideStartingRow + direction, column);
            if (!middle.isOccupied() && !startPlusTwo.isOccupied()) {
                possibleMoveFields.add(startPlusTwo);
            }
        }
    }

    private int checkNextRow(ArrayList<Field> possibleMoveFields, Field right, Field left, Field middle) {
        int canJump = 0;
        if (middle != null && !middle.isOccupied()) {
            if (left != null && right != null) {
                if (!left.isOccupiedWithEnemyFig(this) && !right.isOccupiedWithEnemyFig(this)) {
                    possibleMoveFields.add(middle);
                }
            } else if (left == null && right != null && !right.isOccupiedWithEnemyFig(this)) {
                possibleMoveFields.add(middle);
            } else if (right == null && left != null && !left.isOccupiedWithEnemyFig(this)) {
                possibleMoveFields.add(middle);
            }
        }
        if (right != null && right.isOccupiedWithEnemyFig(this)) {
            canJump++;
            possibleMoveFields.add(right);
        }
        if (left != null && left.isOccupiedWithEnemyFig(this)) {
            canJump++;
            possibleMoveFields.add(left);
        }
        return canJump;
    }

}
