package chess.board;

import chess.figures.Figure;

/**
 * @author Marek Nesvadba, Zdeněk Doležal (xnesva06, xdolez82)
 * <p>Field of chess board.
 */

public class Field {
    private int row;
    private int column;
    private Figure figure;

    public Field(int row, int column) {
        this.row = row;
        this.column = column;
        this.figure = null;
    }

    public Field(Field field) {
        this.row = field.row;
        this.column = field.column;
        this.figure = Figure.copyFigure(field.figure);
    }

    public Figure getFigure() {
        return this.figure;
    }

    public void setFigure(Figure figure) {
        this.figure = figure;
        if (figure != null) {
            figure.setPosition(row, column);
        }
    }

    public void removeFigure() {
        this.figure = null;
    }

    public boolean isOccupied() {
        return figure != null;
    }

    public boolean isOccupiedWithEnemyFig(Figure fig) {
        return isOccupied() && getFigure().getColor() != fig.getColor();
    }

    public int getColumn() {
        return column;
    }

    public int getRow() {
        return row;
    }

    public String toString() {
        return convertRowToChar() + (row + 1);
    }

    private String convertRowToChar() {
        return String.valueOf((char) (column + 97));
    }
}
