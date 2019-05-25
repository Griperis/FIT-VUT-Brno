package chess.figures;

import chess.board.ChessBoard;
import chess.board.Field;

import java.util.ArrayList;

/**
 * @author Marek Nesvadba, Zdeněk Doležal (xnesva06, xdolez82)
 * <p> Class representing the bishop figure.
 */

public class Bishop extends Figure {
    public Bishop(int row, int column, boolean isOnBoard, FigureColor figureColor) {
        super(row, column, isOnBoard, figureColor, FigureType.Bishop);
    }

    public Bishop(Bishop bishop) {
        super(bishop);
    }

    @Override
    public ArrayList<Field> getPossibleMoveFields(ChessBoard board) {
        ArrayList<Field> possibleMoveFields = new ArrayList<>();

        boolean was_occupied_lu = false;
        boolean was_occupied_ld = false;
        boolean was_occupied_ru = false;
        boolean was_occupied_rd = false;

        for (int i = 1; i < ChessBoard.CHESS_BOARD_SIZE; i++) {

            if (!was_occupied_lu) {
                was_occupied_lu = checkMove(board, possibleMoveFields, row + i, column + i);
            }
            if (!was_occupied_ld) {
                was_occupied_ld = checkMove(board, possibleMoveFields, row - i, column + i);
            }
            if (!was_occupied_ru) {
                was_occupied_ru = checkMove(board, possibleMoveFields, row + i, column - i);
            }
            if (!was_occupied_rd) {
                was_occupied_rd = checkMove(board, possibleMoveFields, row - i, column - i);
            }

        }
        return possibleMoveFields;
    }
}
