package chess.figures;

import chess.board.ChessBoard;
import chess.board.Field;

import java.util.ArrayList;

/**
 * @author Marek Nesvadba, Zdeněk Doležal (xnesva06, xdolez82)
 * <p> Class representing the queen figure.
 */

public class Queen extends Figure {
    public Queen(int row, int column, boolean isOnBoard, FigureColor figureColor) {
        super(row, column, isOnBoard, figureColor, FigureType.Queen);
    }

    public Queen(Queen queen) {
        super(queen);
    }

    @Override
    public ArrayList<Field> getPossibleMoveFields(ChessBoard board) {
        ArrayList<Field> possibleMoveFields = new ArrayList<>();

        boolean was_occupied_lu = false;
        boolean was_occupied_ld = false;
        boolean was_occupied_ru = false;
        boolean was_occupied_rd = false;
        boolean was_occupied_u = false;
        boolean was_occupied_d = false;
        boolean was_occupied_l = false;
        boolean was_occupied_r = false;

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
            if (!was_occupied_u) {
                was_occupied_u = checkMove(board, possibleMoveFields, row + i, column);
            }
            if (!was_occupied_d) {
                was_occupied_d = checkMove(board, possibleMoveFields, row - i, column);
            }
            if (!was_occupied_l) {
                was_occupied_l = checkMove(board, possibleMoveFields, row, column + i);
            }
            if (!was_occupied_r) {
                was_occupied_r = checkMove(board, possibleMoveFields, row, column - i);
            }
        }
        return possibleMoveFields;
    }

}
