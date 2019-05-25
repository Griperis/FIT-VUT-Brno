package chess.gui;

import chess.game.Game;
import javafx.event.ActionEvent;
import javafx.fxml.FXML;
import javafx.fxml.FXMLLoader;
import javafx.scene.control.Label;
import javafx.scene.control.Tab;
import javafx.scene.control.TabPane;
import javafx.scene.layout.Pane;
import javafx.stage.FileChooser;

import java.io.File;
import java.io.IOException;
import java.util.ArrayList;

/**
 * @author Marek Nesvadba, Zdeněk Doležal (xnesva06, xdolez82)
 * <p> Controller for the main window, menu bar and tabs.
 */

public class MainController {

    @FXML
    private TabPane tabPane;
    @FXML
    private Label infoLabel;

    static int gameNo = 1;
    static ArrayList<Game> games = new ArrayList<>();

    /**
     * Loads game from file when button is clicked.
     */
    @FXML
    protected void loadGameClicked(ActionEvent event) {
        FileChooser fileChooser = new FileChooser();
        fileChooser.setInitialDirectory(new File("."));
        File file = fileChooser.showOpenDialog(Main.stage);
        if (file == null) return;
        GameController gameController = newGameClicked(null);
        int gameId = Integer.parseInt(tabPane.getSelectionModel().getSelectedItem().getText().split("\\s+")[1]);
        if (getGameById(gameId).loadGame(file) == false) {
            infoLabel.setText("File could not be loaded!");
        } else {
            System.out.println("File loaded successfully");
        }
        gameController.refreshRecord();
        clearInfo();

    }

    /**
     * Saves game to a file when button is clicked.
     */
    @FXML
    protected void saveGameClicked(ActionEvent event) {
        if (tabPane != null && tabPane.getSelectionModel() != null && tabPane.getSelectionModel().getSelectedItem() != null) {
            FileChooser fileChooser = new FileChooser();
            fileChooser.setInitialDirectory(new File("."));
            File file = fileChooser.showSaveDialog(Main.stage);
            if (file == null) return;
            int gameId = Integer.parseInt(tabPane.getSelectionModel().getSelectedItem().getText().split("\\s+")[1]);
            if (!getGameById(gameId).saveGame(file))
            {
                infoLabel.setText("Save game error!");
            }
            clearInfo();
        } else {
            infoLabel.setText("No game available!");
        }
    }

    /**
     * Creates new tab, with new game when button is clicked.
     */
    @FXML
    protected GameController newGameClicked(ActionEvent event) {
        Tab tab = new Tab();
        FXMLLoader loader = new FXMLLoader(getClass().getResource("GameView.fxml"));
        Pane pane = null;
        try {
            pane = loader.load();
        } catch (IOException e) {
            infoLabel.setText("Cannot create new game!");
        }

        tab.setText("Game " + gameNo++);
        tab.setContent(pane);
        tabPane.getTabs().add(tab);
        tabPane.getSelectionModel().select(tab);
        clearInfo();
        return loader.getController();
    }

    private void clearInfo() {
        infoLabel.setText("");
    }

    /**
     * Creates new instance of game.
     *
     * <p>Creates new instance of game, adds it to list of all games, and returns the instance
     *
     * @return Created game
     */
    public static Game createGame() {
        Game newGame = new Game(gameNo);
        games.add(newGame);
        return newGame;
    }

    /**
     * Finds and returns instance of game
     *
     * @param id id of the searched game
     * @return game
     */
    public static Game getGameById(int id) {
        for (Game g : games) {
            if (g.getGameId() == id) {
                return g;
            }
        }
        return null;
    }
}

