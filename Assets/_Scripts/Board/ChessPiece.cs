using Promorph.Data;
using UnityEngine;
using UnityEngine.Events;

namespace Promorph.Board
{
    public class ChessPiece : MonoBehaviour
    {
        [SerializeField] private PieceData _data;
        public EChessPiece Type => _data.Type;
        public Vector2Int[] MoveSet => _data.MoveSet;
        public Vector2Int[] CaptureSet => _data.CaptureSet;

        private SpriteRenderer _spriteRenderer;

        private bool _isSelected;
        public UnityEvent OnSelected;
        private UnityEvent OnFinishedAction = new();

        public void SetData(PieceData data)
        {
            _data = data;

            _spriteRenderer.sprite = _data.Icon;
        }

        private void ShowSelected()
        {
            _isSelected = true;
            BoardManager.Instance.ShowAvailableMoves(this);
        }

        private void HideSelected()
        {
            _isSelected = false;
            BoardManager.Instance.HideAvailableMoves();
        }

        public bool CheckIsAllyPiece(ChessPiece otherPiece)
        {
            //TODO: Use tag ???
            return otherPiece != null && otherPiece.Type == Type;
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

        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            _spriteRenderer.sprite = _data.Icon;
        }

        void OnEnable()
        {
            OnFinishedAction.AddListener(HideSelected);
        }

        void OnDisable()
        {
            OnFinishedAction.RemoveListener(HideSelected);
        }

        void OnMouseDown()
        {
            if (!_isSelected)
            {
                // Highlight the piece or show possible moves: show outline, show move
                ShowSelected();
                OnSelected?.Invoke();
                Debug.Log($"Selected piece: {_data.Type} at position {transform.position}");
            }
            else
            {
                // Remove highlight or hide possible moves
                HideSelected();
            }
        }
    }
}
