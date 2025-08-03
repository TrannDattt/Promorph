using System.Collections.Generic;
using Promorph.Data;
using UnityEngine;
using UnityEngine.Events;

namespace Promorph.Board
{
    public class ChessPiece : MonoBehaviour
    {
        [SerializeField] private PieceData _data;
        [SerializeField] private EFaction _faction;
        public EChessPiece Type => _data.Type;
        public Vector2Int[] MoveSet => GetMoveSet(_data.MoveSet, EPieceAction.Move);
        public Vector2Int[] CaptureSet => GetMoveSet(_data.CaptureSet, EPieceAction.Capture);

        private SpriteRenderer _spriteRenderer;
        public UnityEvent OnClicked;
        // public UnityEvent<bool> OnClicked;
        private UnityEvent OnFinishedAction = new();

        public void SetData(PieceData data)
        {
            _data = data;
        }

        public void ChangeFaction(EFaction faction)
        {
            _faction = faction;
            _spriteRenderer.sprite = _faction == EFaction.White ? _data.WhiteIcon : _data.BlackIcon; 
        }
        
        // public void Initialize()
        // {
        //     _spriteRenderer = GetComponent<SpriteRenderer>();
        //     _spriteRenderer.sprite = _data.Icon;
        // }

        private Vector2Int[] GetMoveSet(MovePattern pattern, EPieceAction action)
        {
            var moves = new List<Vector2Int>();

            foreach (var step in pattern.Steps)
            {
                if (moves.Contains(step)) continue;
                moves.Add(step);
            }

            foreach (var direction in pattern.Directions)
            {
                for (int i = 1; i <= pattern.MaxDistance; i++)
                {
                    var posMove = new Vector2Int(direction.x * i, direction.y * i);
                    if (moves.Contains(posMove)) continue;
                    if (BoardManager.Instance.CheckTileBlock(this, posMove, out ChessPiece piece) && (CheckIsAllyPiece(piece) || action == EPieceAction.Move))
                    {
                        break;
                    }
                    moves.Add(posMove);
                    Debug.Log($"Add pos {action} move: {posMove}");
                    if (BoardManager.Instance.CheckTileBlock(this, posMove, out piece) && !CheckIsAllyPiece(piece))
                    {
                        break;
                    }
                }

                for (int i = 1; i <= pattern.MaxDistance; i++)
                {
                    var negMove = new Vector2Int(direction.x * -i, direction.y * -i);
                    if (moves.Contains(negMove)) continue;
                    if (BoardManager.Instance.CheckTileBlock(this, negMove, out ChessPiece piece) && (CheckIsAllyPiece(piece) || action == EPieceAction.Move))
                    {
                        break;
                    }
                    moves.Add(negMove);
                    Debug.Log($"Add neg {action} move: {negMove}");
                    if (BoardManager.Instance.CheckTileBlock(this, negMove, out piece) && !CheckIsAllyPiece(piece))
                    {
                        break;
                    }
                }
            }

            // Debug.Log("Move set generated:");
            // foreach (var move in moves)
            // {
            //     Debug.Log($"Move: {move}");
            // }

            return moves.ToArray();
        }

        public bool CheckIsAllyPiece(ChessPiece otherPiece)
        {
            return otherPiece != null && otherPiece._faction == _faction;
        }

        public void Move(Vector2 targetPosition)
        {
            transform.position = targetPosition;
            Debug.Log($"Moved piece: {_data.Type} to position {targetPosition}");
            // Additional logic for moving the piece, such as updating its state or notifying the board

            OnFinishedAction?.Invoke();
        }

        // public void Capture()
        // {
        //     // Logic for capturing the piece, such as removing it from the board or updating its state
        //     Debug.Log($"Captured piece: {_data.Type} at position {transform.position}");

        //     Move(transform.position); // Move to the target position
        // }

        public void BeCaptured()
        {
            // Logic for when the piece is captured, such as removing it from the board
            Debug.Log($"Piece: {_data.Type} has been captured.");
            gameObject.SetActive(false);
        }

        public void OnPieceClicked()
        {
            OnClicked?.Invoke();
        }

        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void OnDestroy()
        {
            OnClicked.RemoveAllListeners();
        }

        // void OnMouseDown()
        // {
        //     _isSelected = !_isSelected;
        //     // OnClicked?.Invoke(_isSelected);
        //     OnClicked?.Invoke();
        //     Debug.Log($"Piece clicked: {_data.Type}, Selected: {_isSelected}");
        // }
    }
}
