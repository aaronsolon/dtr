using System;
using System.Diagnostics; //I want this for the stopwatch class for timing my simple game loop

namespace RockPaperScissors
{
    /// <summary>
    /// This class runs the game logic for a Rock Paper Scissors game.
    /// It's meant to be played locally by two players.
    /// </summary>
    public class RockPaperScissorsManager
    {
        //An enum holding the possible moves
        public enum eMoves { ROCK, PAPER, SCISSORS, UNSUBMITTED }

        //An enum for tracking the state of the game
        public enum eGameState { MENUS, AWAITINGINPUT, REVEALINGRESULTS }

        //An enum for making it easy to pass players as a parameter for the user-facing developer
        public enum ePlayerIdentity { PLAYER1, PLAYER2, NONE } 

        //Player variables
        private RPSPlayer player1;
        private RPSPlayer player2;

        //Other variables
        private eGameState gameState = eGameState.MENUS;
        private int numberOfRounds = 5;
        private float timeAllowedToEnterMove = 3f;
        private float timeScoresDisplayedBeforeNewRoundBegins = 3f;
        private float timeSinceStart = 0;
        private float timeCurrentStateBegan;

        private ePlayerIdentity winnerOfLastRound = ePlayerIdentity.NONE;
        private ePlayerIdentity winnerOfLastGame = ePlayerIdentity.NONE;

        /// <summary>
        /// This should be called at the start of the game to set up the variables before we begin play.
        /// </summary>
        public void InitializeRPSManager()
        {
            player1 = new RPSPlayer();
            player2 = new RPSPlayer();
            PrepNewGame();
        }

        /// <summary>
        /// This is called by the main loop every frame to update the game state
        /// Deltatime is how many seconds have elapsed since the last tick.
        /// </summary>
        public void Tick(float deltaTime)
        {
            timeSinceStart += deltaTime; //Keep track of how long the game has been running.

            switch (gameState)
            {
                //The game is waiting for menu input from the players, this could be waiting for them to push a "start game" button, or adjust settings via menu input to change things like number of rounds or timer durations, or a post-game menu.
                case eGameState.MENUS:
                    //We don't need to do anything on tick when in a menu, this state can be left by calling, for example, EnterAwaitingInputState() via menu input (which I assume is created by the user-facing developer).
                    break;
                //The game is counting down while allowing players to input their moves. Once the timer runs out, their moves will be evaluated
                case eGameState.AWAITINGINPUT:
                    if ((timeSinceStart - timeCurrentStateBegan) >= timeAllowedToEnterMove) //Check if they've run out of time
                    {
                        EvaluateCurrentRound();
                    }
                    break;
                //The game is revealing the results of the last throw before moving on to the next throw or ending the game
                case eGameState.REVEALINGRESULTS:
                    if ((timeSinceStart - timeCurrentStateBegan) >= timeScoresDisplayedBeforeNewRoundBegins)
                    {
                        EnterAwaitingInputState();
                    }
                    break;
            }
        }

        #region Scoring
        private void EvaluateCurrentRound()
        {
            //Update scores if appropriate, if both players used the same move there's no need to update scores
            switch (player1.currentMove)
            {
                case eMoves.ROCK:
                    if (player2.currentMove == eMoves.PAPER) Score(ePlayerIdentity.PLAYER2);
                    else if (player2.currentMove == eMoves.SCISSORS || player2.currentMove == eMoves.UNSUBMITTED) Score(ePlayerIdentity.PLAYER1);
                    break;
                case eMoves.PAPER:
                    if (player2.currentMove == eMoves.SCISSORS) Score(ePlayerIdentity.PLAYER2);
                    else if (player2.currentMove == eMoves.ROCK || player2.currentMove == eMoves.UNSUBMITTED) Score(ePlayerIdentity.PLAYER1);
                    break;
                case eMoves.SCISSORS:
                    if (player2.currentMove == eMoves.ROCK) Score(ePlayerIdentity.PLAYER2);
                    else if (player2.currentMove == eMoves.PAPER || player2.currentMove == eMoves.UNSUBMITTED) Score(ePlayerIdentity.PLAYER1);
                    break;
                case eMoves.UNSUBMITTED:
                    if (player2.currentMove != eMoves.UNSUBMITTED) Score(ePlayerIdentity.PLAYER2);
                    break;
            }

            int roundsToWin = ((numberOfRounds / 2) + 1); //This should be half the rounds rounded up, I.E. if we have 5 rounds this value will be 3, the number needed for a victory.
            if (player1.score == roundsToWin) DeclareWinner(ePlayerIdentity.PLAYER1);
            else if (player2.score == roundsToWin) DeclareWinner(ePlayerIdentity.PLAYER2);
            else //If it's not time to declare a winner yet, just reveal the results of the current round
            {
                EnterRevealingResultsState();
            }
        }

        private void Score(ePlayerIdentity roundWinner)
        {
            if (roundWinner == ePlayerIdentity.PLAYER1)
            {
                player1.score++;
            }
            else
            {
                player2.score++;
            }

            winnerOfLastRound = roundWinner;
        }

        /// <summary>
        /// Declare the winner.
        /// The user-facing developer can expand this function to display this information to the player in better way than writing to the console.
        /// </summary>
        private void DeclareWinner(ePlayerIdentity winningPlayer)
        {
            if (winningPlayer == ePlayerIdentity.PLAYER1)
            {
                Console.WriteLine("Player 1 is the victor!");
            }
            else
            {
                Console.WriteLine("Player 2 is the victor!");
            }

            EnterPostGame();
        }
        #endregion

        #region Input Actions
        /// <summary>
        /// This function should be called when a player gives input on what move they want to throw.
        /// </summary>
        public void SubmitPlayerMove(ePlayerIdentity playerDeclaringMove, eMoves moveToDeclare)
        {
            //Target the player that's changing their move
            RPSPlayer p = player1;
            if (playerDeclaringMove == ePlayerIdentity.PLAYER2) p = player2;

            if (p.currentMove == eMoves.UNSUBMITTED) //Only change their current move if they haven't declared one for this round.
            {
                p.currentMove = moveToDeclare;
            }

            //If both players have submitted moves, evaluate the round early
            if (player1.currentMove != eMoves.UNSUBMITTED && player2.currentMove != eMoves.UNSUBMITTED)
            {
                EvaluateCurrentRound();
            }
        }

        /// <summary>
        /// A function to be called when the players choose to begin the game from a menu.
        /// </summary>
        public void StartGame()
        {
            if (gameState == eGameState.MENUS)
            {
                PrepNewGame();
                gameState = eGameState.AWAITINGINPUT;
                EnterAwaitingInputState();
            }
        }
        #endregion

        #region Game State Managment
        private void PrepNewGame()
        {
            player1.score = 0;
            player1.currentMove = eMoves.UNSUBMITTED;

            player2.score = 0;
            player2.currentMove = eMoves.UNSUBMITTED;
        }

        /// <summary>
        /// Called when we begin awaiting input for a new round
        /// </summary>
        private void EnterAwaitingInputState()
        {
            gameState = eGameState.AWAITINGINPUT;
            player1.currentMove = eMoves.UNSUBMITTED;
            player2.currentMove = eMoves.UNSUBMITTED;
            timeCurrentStateBegan = timeSinceStart;
        }

        /// <summary>
        /// Called when a round concludes and it's time to reveal results.
        /// </summary>
        private void EnterRevealingResultsState()
        {
            Console.WriteLine("Player 1 throws: " + player1.currentMove.ToString() + "     Player 2 throws: " + player2.currentMove.ToString());
            gameState = eGameState.REVEALINGRESULTS;
            timeCurrentStateBegan = timeSinceStart;
        }

        /// <summary>
        /// This is called when the game ends and an ultimate winner is declared.
        /// </summary>
        private void EnterPostGame()
        {
            gameState = eGameState.MENUS;
            winnerOfLastGame = winnerOfLastRound;
        }
        #endregion

        #region Getters and Setters For the User-Facing Developer
        public int GetPlayer1Score()
        {
            return player1.score;
        }

        public int GetPlayer2Score()
        {
            return player2.score;
        }

        /// <summary>
        /// Get the current move of the stipulated player. This will be useful for populating UI when showing results, possibly showing art for rock, paper, or scissors.
        /// </summary>
        public eMoves GetCurrentMove(ePlayerIdentity playerIdentity)
        {
            if (playerIdentity == ePlayerIdentity.PLAYER1)
            {
                return player1.currentMove;
            }
            else
            {
                return player2.currentMove;
            }
        }

        /// <summary>
        /// Returns the winner of last round, could be "NONE" if N/A.
        /// </summary>
        public ePlayerIdentity GetWinnerOfLastRound()
        {
            return winnerOfLastRound;
        }

        /// <summary>
        /// Returns the winner of last game, could be "NONE" if N/A.
        /// </summary>
        public ePlayerIdentity GetWinnerOfLastGame()
        {
            return winnerOfLastGame;
        }

        /// <summary>
        /// This function can be used if the players give some kind of menu input to change the number of rounds they want to play.
        /// </summary>
        public void SetNumberOfRounds(int newNumberOfRounds)
        {
            //Check if the number is even, and disallow that
            if (newNumberOfRounds%2 == 0)
            {
                Console.WriteLine("Warning, you tried to set an even number of rounds for the game. Rock Paper Scissors must have an odd number of rounds to work.");
            }
            else //If it's odd, change the round count
            {
                numberOfRounds = newNumberOfRounds;
            }
        }

        /// <summary>
        /// This function could be called if the players give some kind of input through the menus to adjust how long they want for
        /// each round to last (in seconds). This may help the user-facing developers implementation of menus.
        /// </summary>
        public void SetTimeAllowedToEnterMove(float newTimeAllowed)
        {
            timeAllowedToEnterMove = newTimeAllowed;
        }

        public eGameState GetCurrentGameState()
        {
            return gameState;
        }

        /// <summary>
        /// Returns the time in seconds remaining until a timer expires. The user-facing developer can use this value to populate timer UI.
        /// This function works while awaing input, or while revealing results.
        /// </summary>
        /// <returns></returns>
        public float GetTimeLeftInCurrentState()
        {
            float timeLeft = 9999; //Default to just being a big number. This will be returned if we're in the menus, or it will be changed to the accurate value if we have a timer running

            switch(gameState)
            {
                case eGameState.AWAITINGINPUT:
                    timeLeft = ((timeCurrentStateBegan + timeAllowedToEnterMove) - timeSinceStart);
                    break;
                case eGameState.REVEALINGRESULTS:
                    timeLeft = ((timeCurrentStateBegan + timeScoresDisplayedBeforeNewRoundBegins) - timeSinceStart);
                    break;
            }

            return timeLeft;
        }

        /// <summary>
        /// Can be used by the user-facing developer to get the Player struct if they would rather work with that than use the enum stuff.
        /// They can look at the struct's score and current move directly for their own purposes that way.
        /// </summary>
        public RPSPlayer GetPlayerStruct(ePlayerIdentity playerIdentity)
        {
            if (playerIdentity == ePlayerIdentity.PLAYER1)
            {
                return player1;
            }
            else return player2;
        }

        /// <summary>
        /// Can be used by the user-facing developer to get the Player struct if they would rather work with that than use the enum stuff.
        ///They can look at the struct's score and current move directly for their own purposes that way.
        /// </summary>
        public RPSPlayer GetPlayerStruct(int playerNumber)
        {
            if (playerNumber == 1)
            {
                return player1;
            }
            else if (playerNumber == 2)
            {
                return player2;
            }
            else
            {
                Console.WriteLine(playerNumber.ToString() + " does not corrospond to a player number! Use 1 or 2. Returning player 1 now as a default.");
                return player1;
            }
        }
        #endregion

        public struct RPSPlayer
        {
            public int score;
            public eMoves currentMove;
        }
    }

    /// <summary>
    /// A simple class that implements a game loop when run.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            RockPaperScissorsManager rpsManager = new RockPaperScissorsManager();
            rpsManager.InitializeRPSManager();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            int millisecondsPerTick = 30; //We'll just assume we're running at 30ish fps


            while (true)
            {
                //The user-facing developer can collect input in this loop, and communicate it to the rpsManager using functions in the Input Actions region.

                int millisecondsPassed = watch.Elapsed.Milliseconds;
                if (millisecondsPassed >= millisecondsPerTick)
                {
                    watch.Restart();
                    rpsManager.Tick((float)millisecondsPassed / 1000);
                }
            }
        }
    }
}
