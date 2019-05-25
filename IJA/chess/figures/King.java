package chess.figures;

import chess.board.ChessBoard;
import chess.board.Field;

import java.util.ArrayList;
import java.util.HashSet;

/**
 * @author Marek Nesvadba, Zdeněk Doležal (xnesva06, xdolez82)
 * <p> Class representing the king figure.
 */

public class King extends Figure {

    public King(int row, int column, boolean isOnBoard, FigureColor figureColor) {
        super(row, column, isOnBoard, figureColor, FigureType.King);
    }

    public King(King king) {
        super(king);
    }

    @Override
    public ArrayList<Field> getPossibleMoveFields(ChessBoard board) {
        ArrayList<Field> possibleMoveFields = getBasePossibleMoveFields(board);

        HashSet<Field> possibleMovesOfOthers = getPossibleMovesOfOthers(board);
        for (Field other_field : possibleMovesOfOthers) {
            possibleMoveFields.remove(other_field);
        }

        return possibleMoveFields;
    }

    public ArrayList<Field> getBasePossibleMoveFields(ChessBoard board) {
        ArrayList<Field> possibleMoveFields = new ArrayList<>();

        checkMove(board, possibleMoveFields, row + 1, column);
        checkMove(board, possibleMoveFields, row - 1, column);
        checkMove(board, possibleMoveFields, row, column + 1);
        checkMove(board, possibleMoveFields, row, column - 1);
        checkMove(board, possibleMoveFields, row + 1, column + 1);
        checkMove(board, possibleMoveFields, row - 1, column - 1);
        checkMove(board, possibleMoveFields, row - 1, column + 1);
        checkMove(board, possibleMoveFields, row + 1, column - 1);

        return possibleMoveFields;
    }

    private HashSet<Field> getPossibleMovesOfOthers(ChessBoard board) {
        HashSet<Field> possibleMovesOfOthers = new HashSet<>();
        Figure figure;
        ArrayList<Field> possibleMoves = new ArrayList<>();
        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                figure = board.getField(x, y).getFigure();
                if (figure != null && figure.getColor() != this.figureColor) {
                    if (figure.getType() == FigureType.King) {
                        King fig = (King) figure;
                        possibleMoves.addAll(fig.getBasePossibleMoveFields(board));
                        possibleMovesOfOthers.addAll(possibleMoves);
                        break;
                    }
                    possibleMoves = figure.getPossibleMoveFields(board);
                    possibleMovesOfOthers.addAll(possibleMoves);
                }
            }
        }
        return possibleMovesOfOthers;
    }


}
