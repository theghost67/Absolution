using DG.Tweening;
using Game;
using Game.Palette;
using MyBox;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Backgrounds
{
    // works only with aspect ratio equal to 16:9
    // works only with non-movable camera (adjust squares BG object so it would be always on camera center)

    /// <summary>
    /// Класс для заднего фона "перемещение квадратов по сетке".
    /// </summary>
    public class SquaresBackground : MonoBehaviour
    {
        const int TWEENS_MAX = 16;

        const float START_POS_X = -3.185f;
        const float START_POS_Y = 1.82f;

        const float END_POS_X = -START_POS_X;
        const float END_POS_Y = -START_POS_Y;

        const int GRID_SIZE_X = 14; // must be even
        const int GRID_SIZE_Y = 8;  // must be even
        const int GRID_SIZE = GRID_SIZE_X * GRID_SIZE_Y;

        const float SQUARE_LIGHT_UP_DELAY = 1f;
        const float SQUARE_LIGHT_UP_DURATION = 3f;
        const int   SQUARE_LIGHT_UP_COUNT = 2;

        const float SQUARE_START_X = -5.94f;
        const float SQUARE_START_Y = -3.14f;
        const float SQUARES_DISTANCE = 0.91f;
        const float SQUARES_VISIBLE_REFRESH_RATE = 0.1f; // seconds

        // readonly (set in Start())
        Vector3 _startPosScaled;
        Vector3 _speed;

        GameObject _squarePrefab;
        GameObject _gameObject;
        Transform _transform;
        // ------------------------

        Color _lightColor;
        Color _defaultColor;

        SpriteRenderer[,] _squares;
        int2[] _squaresLastLightedUpIndexes;

        Tween[] _tweens;
        Stack<int> _tweensFreeIndexes;

        int2 _visibleSquareRangeX; // equal to half of the grid width
        int2 _visibleSquareRangeY; // equal to half of the grid height
        float _squareLightUpDelay;

        void Start()
        {
            _startPosScaled = new Vector3(START_POS_X, START_POS_Y) * Global.PIXEL_SCALE;
            _speed = new Vector2(15.75f, -9f); // grid size is imperfect, so it's not 16

            _tweens = new Tween[TWEENS_MAX];
            _tweensFreeIndexes = new Stack<int>(TWEENS_MAX);
            for (int i = 0; i < TWEENS_MAX; i++)
                _tweensFreeIndexes.Push(i);

            _squareLightUpDelay = SQUARE_LIGHT_UP_DELAY;
            _squarePrefab = Resources.Load<GameObject>("Prefabs/Backgrounds/Squares/Square");
            _gameObject = gameObject;
            _transform = _gameObject.transform;
            _transform.position = _startPosScaled;

            CreateSquares();
            InvokeRepeating(nameof(UpdateVisibleSquares), 0, SQUARES_VISIBLE_REFRESH_RATE);

            OnColorPaletteChanged();
            ColorPalette.OnPaletteChanged += OnColorPaletteChanged;
        }
        void Update()
        {
            float delta = Time.deltaTime;

            _transform.position += _speed * delta;
            if (_transform.position.x >= END_POS_X * Global.PIXEL_SCALE &&
                _transform.position.y <= END_POS_Y * Global.PIXEL_SCALE)
            {
                _transform.position = _startPosScaled;
                SwitchVisibleSquares();
            }

            _squareLightUpDelay -= delta;
            if (_squareLightUpDelay > 0) return;

            LightUpSquare();
            _squareLightUpDelay = SQUARE_LIGHT_UP_DELAY;
        }

        void OnColorPaletteChanged()
        {
            _lightColor = ColorPalette.GetColor(1).WithAlpha(0.75f);
            _defaultColor = ColorPalette.GetColor(3).WithAlpha(0.75f);

            for (int y = 0; y < GRID_SIZE_Y; y++)
            {
                for (int x = 0; x < GRID_SIZE_X; x++)
                    _squares[x, y].color = _defaultColor;
            }

            for (int i = 0; i < TWEENS_MAX; i++)
            {
                Tween tween = _tweens[i];
                if (tween == null || !tween.active) continue;
                _tweens[i].onUpdate();

                //SpriteRenderer square = (SpriteRenderer)tween.target;
                //float elapsed = tween.Elapsed();
                //if (tween.IsComplete()) continue;

                //tween.Kill();
                //tween = CreateSquareTween(square);
                //tween.Goto(elapsed, andPlay: true);
                //_tweens[i] = tween;
            }
        }

        void UpdateVisibleSquares()
        {
            _visibleSquareRangeX = GetVisibleSquaresRangeX();
            _visibleSquareRangeY = GetVisibleSquaresRangeY();
        }
        int2 GetVisibleSquaresRangeX()
        {
            const float SQUARE_ENTRY_X_MIN = -3.565f;
            const int SQUARES_FIT_HORIZONTALLY = 7;

            int squareXMinIndex = 0;
            int squareXMaxIndex = 0;

            for (int x = 0; x < GRID_SIZE_X; x++)
            {
                float squareX = _squares[x, 0].transform.position.x;
                if (squareX < SQUARE_ENTRY_X_MIN * Global.PIXEL_SCALE) continue;

                squareXMinIndex = x;
                squareXMaxIndex = Mathf.Clamp(x + SQUARES_FIT_HORIZONTALLY, 0, GRID_SIZE_X);
                break;
            }
            return new int2(squareXMinIndex, squareXMaxIndex);
        }
        int2 GetVisibleSquaresRangeY()
        {
            const float SQUARE_ENTRY_Y_MIN = -2.165f;
            const int SQUARES_FIT_VERICALLY = 4;

            int squareYMinIndex = 0;
            int squareYMaxIndex = 0;

            for (int y = 0; y < GRID_SIZE_Y; y++)
            {
                float squareY = _squares[0, y].transform.position.y;
                if (squareY < SQUARE_ENTRY_Y_MIN * Global.PIXEL_SCALE) continue;

                squareYMinIndex = y;
                squareYMaxIndex = Mathf.Clamp(y + SQUARES_FIT_VERICALLY, 0, GRID_SIZE_Y);
                break;
            }
            return new int2(squareYMinIndex, squareYMaxIndex);
        }

        void CreateSquares()
        {
            _squares = new SpriteRenderer[GRID_SIZE_X, GRID_SIZE_Y];
            for (int y = 0; y < GRID_SIZE_Y; y++)
            {
                for (int x = 0; x < GRID_SIZE_X; x++)
                {
                    GameObject instance = Instantiate(_squarePrefab, _gameObject.transform);
                    SpriteRenderer renderer = instance.GetComponent<SpriteRenderer>();

                    instance.name = $"Square [{x + 1}, {y + 1}]";
                    instance.transform.localPosition = GetSquarePos(x, y);
                    _squares[x, y] = renderer;
                }
            }
        }
        void SwitchVisibleSquares()
        {
            // switches visible squares positions between START_POS and END_POS
            // (used to keep squares animations)
            const int HALF_SIZE_X = GRID_SIZE_X / 2;
            const int HALF_SIZE_Y = GRID_SIZE_Y / 2;

            SpriteRenderer[,] invisibles = new SpriteRenderer[HALF_SIZE_X, HALF_SIZE_Y];
            for (int y = 0; y < HALF_SIZE_Y; y++) // fill with invisible squares to use later (_squares will be overwritten)
            {
                for (int x = 0; x < HALF_SIZE_X; x++)
                    invisibles[x, y] = _squares[x + HALF_SIZE_X, y];
            }

            // move currently visible squares
            for (int y = HALF_SIZE_Y; y < GRID_SIZE_Y; y++)
            {
                for (int x = 0; x < HALF_SIZE_X; x++)
                {
                    int newX = x + HALF_SIZE_X;
                    int newY = y - HALF_SIZE_Y;
                    SpriteRenderer square = _squares[x, y];
                    square.transform.localPosition = GetSquarePos(newX, newY);
                    _squares[newX, newY] = square;
                }
            }

            // move invisible squares
            for (int y = 0; y < HALF_SIZE_Y; y++)
            {
                for (int x = 0; x < HALF_SIZE_X; x++)
                {
                    int newX = x;
                    int newY = y + HALF_SIZE_Y;
                    SpriteRenderer square = invisibles[x, y];
                    square.transform.localPosition = GetSquarePos(newX, newY);
                    _squares[newX, newY] = square;
                }
            }
        }
        void LightUpSquare()
        {
            _squaresLastLightedUpIndexes = new int2[SQUARE_LIGHT_UP_COUNT].FillBy(i => new int2(-1, -1));
            for (int i = 0; i < SQUARE_LIGHT_UP_COUNT; i++)
            {
                if (_tweensFreeIndexes.Count == 0)
                {
                    Debug.LogWarning($"{nameof(SquaresBackground)}: There are no indexes available for a new tween.");
                    return;
                }

                TryAgain:
                int2 squarePos = GetRandomVisibleSquarePos();
                if (_squaresLastLightedUpIndexes.Contains(squarePos))
                    goto TryAgain;

                SpriteRenderer square = _squares[squarePos.x, squarePos.y];
                _squaresLastLightedUpIndexes[i] = squarePos;
                square.DOKill();

                int index = _tweensFreeIndexes.Pop();
                Tween tween = CreateSquareTween(square);
                tween.OnComplete(() => _tweensFreeIndexes.Push(index));
                _tweens[index] = tween;
            }
        }

        int2 GetRandomVisibleSquarePos()
        {
            int2 pos = int2.zero;
            if (_visibleSquareRangeX.y == 0) // not initialized
                return pos;

            pos.x = UnityEngine.Random.Range(_visibleSquareRangeX.x, _visibleSquareRangeX.y);
            pos.y = UnityEngine.Random.Range(_visibleSquareRangeY.x, _visibleSquareRangeY.y);
            return pos;
        }
        Vector3 GetSquarePos(int x, int y)
        {
            return new Vector3(SQUARE_START_X + SQUARES_DISTANCE * x, SQUARE_START_Y + SQUARES_DISTANCE * y);
        }
        Tween CreateSquareTween(SpriteRenderer square)
        {
            square.color = _lightColor;
            return DOVirtual.Float(0, 1, SQUARE_LIGHT_UP_DURATION, v => square.color = Color.Lerp(_lightColor, _defaultColor, v));
            //return square.DOColor(_defaultColor, SQUARE_LIGHT_UP_DURATION).SetEase(Ease.OutQuad);
        }
    }
}
