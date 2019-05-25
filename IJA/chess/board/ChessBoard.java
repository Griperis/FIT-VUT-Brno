package chess.board;

import chess.figures.*;
import com.sun.javafx.font.freetype.FTFactory;

import java.util.ArrayList;

/**
 * @author Marek Nesvadba, Zdeněk Doležal (xnesva06, xdolez82)
 * <p>Class representing the whole chess board.
 */

public class ChessBoard {
    public static final int CHESS_BOARD_SIZE = 8;
    private Field[][] board;

    public ChessBoard() {
        board = new Field[CHESS_BOARD_SIZE][CHESS_BOARD_SIZE];
        createBoard();
        addFigures();
    }

    public Field getField(int row, int column) {
        if (row >= 0 && column >= 0 && row < CHESS_BOARD_SIZE && column < CHESS_BOARD_SIZE) {
            return this.board[row][column];
        } else {
            return null;
        }
    }

    public void setField(Field field) {
        board[field.getRow()][field.getColumn()] = field;
    }
    public ArrayList<Figure> getAllFiguresOfType(FigureType type){
        ArrayList<Figure> figures = new ArrayList<>();
        for (int i = 0; i < CHESS_BOARD_SIZE; i++){
            for (int j = 0; j < CHESS_BOARD_SIZE; j++){
                Figure fig = board[i][j].getFigure();
                if (fig != null && fig.getType() == type) {
                    figures.add(fig);
                }
            }
        }
        return figures;
    }
    public Field getFieldWithFigFromRow(int row, FigureType type)
    {
        for (int i = 0; i < CHESS_BOARD_SIZE; i++) {
            Figure fig = board[row][i].getFigure();
            if (fig != null && fig.getType() == type) {
                return board[row][i];
            }
        }
        return null;
    }

    public Field getFieldWithFigFromCol(int col, FigureType type)
    {
        for (int i = 0; i < CHESS_BOARD_SIZE; i++) {
            Figure fig = board[i][col].getFigure();
            if (fig != null && fig.getType() == type){
                return board[i][col];
            }
        }
        return null;
    }
    private void createBoard() {
        for (int i = 0; i < CHESS_BOARD_SIZE; i++) {
            for (int j = 0; j < CHESS_BOARD_SIZE; j++) {
                board[i][j] = new Field(i, j);
            }
        }
    }

    /**
     * Fills board with figures
     */
    private void addFigures() {
        // Add pawns
        for (int i = 0; i < CHESS_BOARD_SIZE; i++) {
            this.board[1][i].setFigure(new Pawn(1, i, true, FigureColor.White));
            this.board[6][i].setFigure(new Pawn(6, i, true, FigureColor.Black));
        }

        // Add rooks for white side
        this.board[0][0].setFigure(new Rook(0, 0, true, FigureColor.White));
        this.board[0][7].setFigure(new Rook(0, 7, true, FigureColor.White));

        // Add rooks for black side
        this.board[7][0].setFigure(new Rook(7, 0, true, FigureColor.Black));
        this.board[7][7].setFigure(new Rook(7, 7, true, FigureColor.Black));

        // Add knights for white side
        this.board[0][1].setFigure(new Knight(0, 1, true, FigureColor.White));
        this.board[0][6].setFigure(new Knight(0, 6, true, FigureColor.White));

        // Add knights for black side
        this.board[7][1].setFigure(new Knight(7, 1, true, FigureColor.Black));
        this.board[7][6].setFigure(new Knight(7, 6, true, FigureColor.Black));

        // Add bishops for white side
        this.board[0][2].setFigure(new Bishop(0, 2, true, FigureColor.White));
        this.board[0][5].setFigure(new Bishop(0, 5, true, FigureColor.White));

        // Add bishops for black side
        this.board[7][2].setFigure(new Bishop(7, 2, true, FigureColor.Black));
        this.board[7][5].setFigure(new Bishop(7, 5, true, FigureColor.Black));

        // Add queen and king for white side
        this.board[0][3].setFigure(new Queen(0, 3, true, FigureColor.White));
        this.board[0][4].setFigure(new King(0, 4, true, FigureColor.White));

        // Add queen and king for black side
        this.board[7][3].setFigure(new Queen(7, 3, true, FigureColor.Black));
        this.board[7][4].setFigure(new King(7, 4, true, FigureColor.Black));

    }

    public String toString() {
        StringBuilder result = new StringBuilder();
        for (int i = 0; i < CHESS_BOARD_SIZE; i++) {
            for (int j = 0; j < CHESS_BOARD_SIZE; j++) {
                result.append(this.board[i][j].toString()).append("\t");
            }
            result.append("\n");
        }
        return result + "\n";
    }
}

