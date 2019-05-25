package chess.gui;

import chess.figures.Figure;
import chess.figures.FigureColor;
import javafx.embed.swing.SwingFXUtils;
import javafx.scene.control.Button;
import javafx.scene.image.Image;
import javafx.scene.image.ImageView;

import javax.imageio.ImageIO;
import java.io.File;
import java.io.IOException;

/**
 * @author Marek Nesvadba, Zdeněk Doležal (xnesva06, xdolez82)
 * <p>Field of chess board used in GUI.
 */

public class GuiBoardField extends Button {

    private int col;
    private int row;

    private boolean isEnabled;

    private Figure figure = null;


    private String base_style = "-fx-font-size: 50px; -fx-background-radius: 0; -fx-padding: 0;  ";
    private String current_style = "";

    private File image_file;
    private Image figure_image;

    GuiBoardField(int col, int row) {
        this.col = col;
        this.row = row;
        setBaseStyle();
    }

    GuiBoardField(Figure figure) {
        this.col = figure.getColumn();
        this.row = figure.getRow();
        this.figure = figure;
        this.setText(figure.toString());
        String path = "lib/";
        if (this.figure.getColor() == FigureColor.White) {
            switch (this.figure.getType()) {
                case Bishop:
                    image_file = new File(path + "bishop_w.png");
                    break;
                case King:
                    image_file = new File(path + "king_w.png");
                    break;
                case Knight:
                    image_file = new File(path + "knight_w.png");
                    break;
                case Pawn:
                    image_file = new File(path + "pawn_w.png");
                    break;
                case Queen:
                    image_file = new File(path + "queen_w.png");
                    break;
                case Rook:
                    image_file = new File(path + "tower_w.png");
                    break;
            }
        } else {
            switch (this.figure.getType()) {
                case Bishop:
                    image_file = new File(path + "bishop_b.png");
                    break;
                case King:
                    image_file = new File(path + "king_b.png");
                    break;
                case Knight:
                    image_file = new File(path + "knight_b.png");
                    break;
                case Pawn:
                    image_file = new File(path + "pawn_b.png");
                    break;
                case Queen:
                    image_file = new File(path + "queen_b.png");
                    break;
                case Rook:
                    image_file = new File(path + "tower_b.png");
                    break;
            }
        }

        try {
            figure_image = SwingFXUtils.toFXImage(ImageIO.read(image_file), null);
        } catch (IOException e) {
            e.printStackTrace();
        }

        ImageView imageView = new ImageView(figure_image);
        imageView.setPreserveRatio(true);
        imageView.setFitHeight(GameController.fieldSize - 10);
        imageView.setFitWidth(GameController.fieldSize - 10);
        this.setGraphic(imageView);

        setBaseStyle();
    }

    private void setBaseStyle() {

        current_style = base_style;

        if (((col % 2 == 1) && (row % 2 == 0)) || ((col % 2 == 0) && (row % 2 == 1))) {
            current_style += "-fx-base:GRAY;";
        }

        base_style = current_style;
        this.setStyle(current_style);
        updateProperties();
    }

    private void updateProperties() {
        if (isEnabled) {
            this.setStyle(current_style + "-fx-border-color: ORANGE;-fx-border-width:3;");
        } else {
            this.setStyle(base_style);
        }

        this.setMaxSize(GameController.fieldSize, GameController.fieldSize);
        this.setMinSize(GameController.fieldSize, GameController.fieldSize);

    }

    void setEnabled(boolean enabled) {
        this.isEnabled = enabled;
        updateProperties();
    }

    boolean getEnabled() {
        return this.isEnabled;
    }

    public int getCol() {
        return col;
    }

    public int getRow() {
        return row;
    }

    public void setFigure(Figure figure) {
        this.figure = figure;
    }

    public Figure getFigure() {
        return figure;
    }

    @Override
    public String toString() {
        if (this.figure != null) {
            return "Col: " + this.col + ",Row: " + this.row + " " + this.figure.toString();
        } else {
            return "Col: " + this.col + ",Row: " + this.row;
        }

    }
}
