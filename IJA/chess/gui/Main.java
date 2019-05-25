package chess.gui;

import javafx.application.Application;
import javafx.fxml.FXMLLoader;
import javafx.scene.Parent;
import javafx.scene.Scene;
import javafx.stage.Stage;

/**
 * @author Marek Nesvadba, Zdeněk Doležal (xnesva06, xdolez82)
 * <p>Main class used to start the whole application.
 */

public class Main extends Application {

    public static Stage stage;

    @Override
    public void start(Stage primaryStage) throws Exception {
        stage = primaryStage;
        Parent root = FXMLLoader.load(getClass().getResource("MainView.fxml"));
        primaryStage.setTitle("IJA - Chess");
        primaryStage.setScene(new Scene(root, 1000, 950));
        primaryStage.setMinHeight(950);
        primaryStage.setMinWidth(1000);
        primaryStage.show();
    }

    /**
     * Entry point of the program.
     *
     * @param args program arguments
     */
    public static void main(String[] args) {
        launch(args);
    }
}
