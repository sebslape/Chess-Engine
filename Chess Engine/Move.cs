using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Engine
{
    public class Move
    {
        private int fFrom;
        private int fTo;
        private Piece fPieceFrom;
        private Piece fPieceTo;
        private Piece fPieceCaptured;
        private int fFlags;
        private int fScore;
        private int fEp;
        private int fCastle;
        private int fPly;
        private int fHalfMoveClock;

        public Move(int from, int to, Piece pieceFrom, Piece pieceCaptured, int flags) {
            //-- Set by move --\\
            fFrom = from;
            fTo = to;
            fPieceFrom = pieceFrom;

            // This is only different to piece_from when
            // the move is a promotion
            fPieceTo = pieceFrom;

            // We need this one in case of capture promotions where you have
            // a "from" piece (lets say a pawn) a captured piece (lets say a queen)
            // and a "to" piece (the pawn's promotion piece like a queen or a knight)
            // We need all three of these to be able to restore the previous state
            fPieceCaptured = pieceCaptured;
            
            fFlags = flags;

            fScore = 0;

            //-- Set by board --\\
            fEp = Board.ep;
            fCastle = Board.castle;
            fPly = Board.ply;
            fHalfMoveClock = Board.halfMoveClock;
        }

        // Get/Set
        public int From
        {
            get { return fFrom; }
            set { fFrom = value; }
        }
        public int To
        {
            get { return fTo; }
            set { fTo = value; }
        }
        public Piece PieceFrom
        {
            get { return fPieceFrom; }
            set { fPieceFrom = value; }
        }
        public Piece PieceTo
        {
            get { return fPieceTo; }
            set { fPieceTo = value; }
        }
        public Piece PieceCaptured
        {
            get { return fPieceCaptured; }
            set { fPieceCaptured = value; }
        }
        public int Flags
        {
            get { return fFlags; }
            set { fFlags = value; }
        }
        public int Score
        {
            get { return fScore; }
            set { fScore = value; }
        }
        public int Ep
        {
            get { return fEp; }
            set { fEp = value; }
        }
        public int Castle
        {
            get { return fCastle; }
            set { fCastle = value; }
        }
        public int Ply
        {
            get { return fPly; }
            set { fPly = value; }
        }
        public int HalfMoveClock
        {
            get { return fHalfMoveClock; }
            set { fHalfMoveClock = value; }
        }
    }
}