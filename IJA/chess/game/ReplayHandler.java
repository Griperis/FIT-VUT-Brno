package chess.game;

import chess.gui.GameController;
import javafx.application.Platform;

import java.util.Timer;
import java.util.TimerTask;

/**
 * @author Marek Nesvadba, Zdeněk Doležal (xnesva06, xdolez82)
 * <p>Handles all of the replay features in screen;
 */

public class ReplayHandler {
    private Record playerRecord;
    private Record loadedRecord;
    private Game game;
    private boolean playerMoved;
    private boolean wasUndo;
    private boolean returnedToLoaded;
    private boolean autoPlay;
    TimerTask autoPlayerTask;
    Timer autoPlayerTimer;
    private int completeRecordSize;
    public ReplayHandler(Record playerRecord, Record loadedRecord, Game game) {
        this.playerRecord = playerRecord;
        this.loadedRecord = loadedRecord;
        this.game = game;
        this.playerMoved = false;
        this.wasUndo = false;
        this.returnedToLoaded = true;
        this.autoPlay = false;
        this.completeRecordSize = 0;
    }

    protected void undoPlayerMove() {
        if (!autoPlay) {
            Move move = playerRecord.getPrevMove();
            if (move != null) {
                game.chessBoard.setField(move.sourceField);
                game.chessBoard.setField(move.destField);
                wasUndo = true;
                game.setCheckmate(move.wasCheckMate());
                game.setCheck(move.sourceField.getFigure().getColor(), move.wasCheck());
                game.changeTurn();
            }
            if (playerRecord.getIndex() == 0 && loadedRecord.getSize() > 0) {
                loadedRecord.resetMaxIndex();
                returnedToLoaded = true;
            }
        }
    }

    protected void lockLoadedMovesIndex() {
        returnedToLoaded = false;
        playerMoved = true;
        loadedRecord.setMaxIndex(loadedRecord.getIndex());
        if (wasUndo) {
            playerRecord.setMaxIndex(playerRecord.getIndex());
            wasUndo = false;
        }
    }

    protected void redoPlayerMove() {
        if (!autoPlay) {
            Move move = playerRecord.getNextMove();
            if (move != null) {
                if (wasUndo)
                {
                    loadedRecord.setMaxIndex(loadedRecord.getIndex());
                    playerRecord.resetMaxIndex();
                }
                game.chessBoard.setField(move.sourceFieldAfter);
                game.chessBoard.setField(move.destFieldAfter);
                wasUndo = false;
                returnedToLoaded = false;
                game.setCheckmate(move.wasCheckMate());
                game.setCheck(move.destFieldAfter.getFigure().getColor(), move.wasCheck());
                game.changeTurn();
            }
        }
    }

    public void playAutomatically(int delay, boolean forward, GameController gc) {
        if (!autoPlay)
        {
            autoPlay = true;
            autoPlayerTimer = new Timer();
            autoPlayerTask = new TimerTask() {
                @Override
                public void run() {
                    Platform.runLater(()-> {;
                        if (forward) {
                            playNextHalfMove();
                        }else {
                            playPreviousHalfMove();
                        }
                        if (getCompleteRecordIndex() <= 0) {
                            pauseAutomaticPlayer();
                        }
                        if (forward && getCompleteRecordIndex() >= completeRecordSize) {
                            playNextHalfMove();
                            pauseAutomaticPlayer();
                        }
                        gc.refreshFields();
                        gc.refreshRecord();
                        gc.updateLabel();
                        });
                }
            };
            autoPlayerTimer.schedule(autoPlayerTask, 0, delay);
        }
    }

    public void pauseAutomaticPlayer() {
        if (autoPlay)
        {
            autoPlayerTimer.cancel();
            autoPlayerTask.cancel();
            autoPlay = false;
        }
    }

    public void playNextHalfMove() {
        Move move = loadedRecord.getNextMove();
        if (move != null){
            game.chessBoard.setField(move.sourceFieldAfter);
            game.chessBoard.setField(move.destFieldAfter);
            playerRecord.resetMaxIndex();
            game.changeTurn();
            game.setCheckmate(move.wasCheckMate());
        } else {
            move = playerRecord.getNextMove();
            if (move != null) {
                game.chessBoard.setField(move.sourceFieldAfter);
                game.chessBoard.setField(move.destFieldAfter);
                game.changeTurn();
                game.setCheckmate(move.wasCheckMate());
                game.setCheck(move.destFieldAfter.getFigure().getColor(), move.wasCheck());
            }
        }
        resetPlayerMoves();
    }

    public void playPreviousHalfMove() {
        Move move = playerRecord.getPrevMove();
        if (move != null && playerMoved) {
            game.chessBoard.setField(move.sourceField);
            game.chessBoard.setField(move.destField);
            playerRecord.resetMaxIndex();
            game.changeTurn();
            game.setCheckmate(move.wasCheckMate());
            resetPlayerMoves();
        } else {
            move = loadedRecord.getPrevMove();
            if (move != null) {
                game.chessBoard.setField(move.sourceField);
                game.chessBoard.setField(move.destField);
                game.changeTurn();
                game.setCheckmate(move.wasCheckMate());
                game.setCheck(move.sourceField.getFigure().getColor(), move.wasCheck());
                resetPlayerMoves();
            }
        }
    }

    public void restartPlayer() {
        if (autoPlay)
        {
            pauseAutomaticPlayer();
        }
        movePlayerTo(0);
    }

    public void movePlayerTo(int index) {
        while (getCompleteRecordIndex() != index) {
            int i = getCompleteRecordIndex();
            if (i < index) {
                playNextHalfMove();
                playNextHalfMove();
            }
            if (i > index) {
                playPreviousHalfMove();
                playPreviousHalfMove();
            }
        }
    }

    public int getCompleteRecordIndex() {
        return (int) Math.ceil(((double) loadedRecord.getIndex() + playerRecord.getIndex()) / 2.0);
    }

    public Record getCompleteRecord() {
        Record validLoadedRecord = loadedRecord.getValidPart();
        if (playerMoved && !returnedToLoaded) {
            Record.append(validLoadedRecord, playerRecord);
        }
        completeRecordSize = (int)Math.ceil(((double)validLoadedRecord.getSize()) / 2.0);
        return validLoadedRecord;
    }
    public boolean isAutoPlayOn()
    {
        return autoPlay;
    }
    private void resetPlayerMoves() {
        if (returnedToLoaded) {
            playerRecord.clear();
        }
    }

}