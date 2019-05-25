package chess.game;

import chess.board.ChessBoard;
import chess.board.Field;
import chess.figures.Figure;
import chess.figures.FigureColor;
import chess.figures.FigureType;
import chess.figures.King;
import chess.io.FileHandler;

import java.io.File;
import java.util.ArrayList;
import java.util.HashSet;

/**
 * @author Marek Nesvadba, Zdeněk Doležal (xnesva06, xdolez82)
 * <p>Keeps track of the game and stores all information about it.
 */

public class Game {
    ChessBoard chessBoard;
    private Record playerRecord;
    private Record loadedRecord;
    private ReplayHandler replayHandler;
    private FileHandler fileHandler;
    private int id;
    private FigureColor turnColor;
    private boolean isWhiteCheck = false;
    private boolean isBlackCheck = false;
    private boolean isCheckmate = false;
    public boolean wasWhiteCheck = false;
    public boolean wasBlackCheck = false;

    public Game(int gameId) {
        System.out.println("Creating game: " + gameId);
        this.id = gameId;
        this.chessBoard = new ChessBoard();
        this.playerRecord = new Record();
        this.loadedRecord = new Record();
        this.replayHandler = new ReplayHandler(this.playerRecord, this.loadedRecord, this);
        this.fileHandler = new FileHandler();
        this.turnColor = FigureColor.White;
    }

    public boolean loadGame(File file) {
        boolean retVal = fileHandler.loadRecord(file, loadedRecord);
        loadedRecord.resetIndex();
        return retVal;
    }

    public boolean saveGame(File file) {
        return fileHandler.saveRecord(replayHandler.getCompleteRecord(), file);
    }

    public boolean isOnTurn(FigureColor color) {
        return color == turnColor;
    }

    public void move(Figure selectedFigure, Field destination, FigureType type) {
        if (type != null) {
            promotion(selectedFigure, destination, type);
            return;
        }
        Field figurePosition = chessBoard.getField(selectedFigure.getRow(), selectedFigure.getColumn());
        Move move = new Move(figurePosition, destination);
        ArrayList<Move.Tag> tags = new ArrayList<>();
        replayHandler.lockLoadedMovesIndex();
        playerRecord.addMove(move);
        addKickTag(selectedFigure, destination, tags);
        destination.setFigure(selectedFigure);
        figurePosition.removeFigure();

        wasBlackCheck = getCheck(FigureColor.Black);
        wasWhiteCheck = getCheck(FigureColor.White);
        checkCheck();
        addCheckTags(selectedFigure, tags);
        move.executeMove(figurePosition, destination, tags.toArray(new Move.Tag[0]));
        changeTurn();

    }

    private void addCheckTags(Figure selectedFigure, ArrayList<Move.Tag> tags) {
        if (getCheck(getOpositeColor(selectedFigure.getColor()))) {
            tags.add(Move.Tag.Check);
        }
        if (wasBlackCheck && getCheck(FigureColor.Black)) {
            setCheckmate(true);
            tags.add(Move.Tag.CheckMate);

        } else if (wasWhiteCheck && getCheck(FigureColor.White)) {
            setCheckmate(true);
            tags.add(Move.Tag.CheckMate);
        }
    }

    private void promotion(Figure selectedFigure, Field destination, FigureType type) {
        Field figurePosition = chessBoard.getField(selectedFigure.getRow(), selectedFigure.getColumn());
        Move move = new Move(figurePosition, destination);
        ArrayList<Move.Tag> tags = new ArrayList<>();
        replayHandler.lockLoadedMovesIndex();
        playerRecord.addMove(move);
        addKickTag(selectedFigure, destination, tags);
        tags.add(Move.Tag.Promotion);
        Figure figure = Figure.promotePawn(destination.getRow(), destination.getColumn(), true, selectedFigure.getColor(), type);
        destination.setFigure(figure);
        figurePosition.removeFigure();

        addCheckTags(selectedFigure, tags);
        move.executeMove(figurePosition, destination, tags.toArray(new Move.Tag[0]));
        changeTurn();
    }

    private void addKickTag(Figure selectedFigure, Field field, ArrayList<Move.Tag> tags) {
        if (field.isOccupiedWithEnemyFig(selectedFigure)) {
            tags.add(Move.Tag.Kick);
        }
    }


    public ArrayList<Field> getPossibleMoves(Figure selectedFigure) {
        return selectedFigure.getPossibleMoveFields(chessBoard);
    }

    public void undoMove() {
        replayHandler.undoPlayerMove();
    }

    public void redoMove() {
        replayHandler.redoPlayerMove();
    }

    public void printGame() {
        System.out.println(this.chessBoard);
    }

    public int getGameId() {
        return this.id;
    }

    public Field getBoardField(int row, int column) {
        return chessBoard.getField(row, column);
    }

    public ReplayHandler getReplayHandler() {
        return replayHandler;
    }

    void changeTurn() {
        if (turnColor == FigureColor.White) {
            turnColor = FigureColor.Black;
        } else if (turnColor == FigureColor.Black) {
            turnColor = FigureColor.White;
        }
    }


    public boolean getCheck(FigureColor color) {
        if (color == FigureColor.Black) {
            return isBlackCheck;
        } else {
            return isWhiteCheck;
        }
    }

    public void setCheck(FigureColor color, boolean check) {
        if (color == FigureColor.Black) {
            this.isBlackCheck = check;
        } else {
            this.isWhiteCheck = check;
        }
    }

    public boolean getCheckmate() {
        return this.isCheckmate;
    }

    public void setCheckmate(boolean checkmate) {
        this.isCheckmate = checkmate;
    }

    private void checkCheck() {
        Field kingBlackField = null;
        Field kingWhiteField = null;

        Figure figure;
        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                figure = chessBoard.getField(x, y).getFigure();
                if (figure != null && figure.getType() == FigureType.King) {
                    if (figure.getColor() == FigureColor.Black) {
                        kingBlackField = chessBoard.getField(figure.getRow(), figure.getColumn());
                    } else {
                        kingWhiteField = chessBoard.getField(figure.getRow(), figure.getColumn());
                    }
                }
            }
        }

        HashSet<Field> possibleMovesOfOthersBlack = getPossibleMovesOfOthers(chessBoard, kingBlackField.getFigure());
        HashSet<Field> possibleMovesOfOthersWhite = getPossibleMovesOfOthers(chessBoard, kingWhiteField.getFigure());

        isBlackCheck = isInDanger(possibleMovesOfOthersBlack, kingBlackField);
        isWhiteCheck = isInDanger(possibleMovesOfOthersWhite, kingWhiteField);

    }

    private boolean isInDanger(HashSet<Field> possibleMoves, Field kingField) {
        for (Field field : possibleMoves) {
            if (field == kingField) {
                return true;
            }
        }
        return false;
    }

    private FigureColor getOpositeColor(FigureColor color) {
        if (color == FigureColor.White) {
            return FigureColor.Black;
        } else {
            return FigureColor.White;
        }
    }

    private HashSet<Field> getPossibleMovesOfOthers(ChessBoard board, Figure king) {
        HashSet<Field> possibleMovesOfOthers = new HashSet<>();
        Figure figure;
        ArrayList<Field> possibleMoves = new ArrayList<>();
        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                figure = board.getField(x, y).getFigure();
                if (figure != null && figure.getColor() != king.getColor()) {
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
