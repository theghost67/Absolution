using DG.Tweening;
using Game.Palette;
using MyBox;
using System.Collections.Generic;
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

        public bool manualLightUp;
        public float lightUpDuration = SQUARE_LIGHT_UP_DURATION;

        // readonly (set in Start())
        Vector3 _startPosScaled;
        Vector3 _speed;

        GameObject _squarePrefab;
        GameObject _gameObject;
        Transform _transform;
        // ------------------------

        Color _lightColor;
        Color _defaultColor;

        bool _initialized;
        Square[,] _squares;
        Square[] _squaresVisible;

        Tween[] _tweens;
        Stack<int> _tweensFreeIndexes;

        int2 _visibleSquareRangeX; // equal to half of the grid width
        int2 _visibleSquareRangeY; // equal to half of the grid height
        float _squareLightUpDelay;

        struct Square
        {
            public readonly SpriteRenderer renderer;
            public readonly Transform transform;
            public readonly int2 pos;
            bool _isLightedUp;

            public Square(SpriteRenderer renderer, int2 pos)
            {
                this.transform = renderer.transform;
                this.renderer = renderer;
                this.pos = pos;
                _isLightedUp = false;
            }
            public void SwitchLightUp()
            {
                _isLightedUp = !_isLightedUp;
            }
            public bool IsLightedUp()
            {
                return _isLightedUp;
            }
        }

        public void LightUpSquares(int count)
        {
            if (!_initialized) return;
            if (_tweensFreeIndexes.Count == 0)
            {
                Debug.LogWarning($"{nameof(SquaresBackground)}: There are no indexes available for a new tween.");
                return;
            }

            Square[] squares = new Square[count];
            List<int2> squaresAvailable = new(_squaresVisible.Length);
            foreach (Square square in _squaresVisible)
            {
                if (!square.IsLightedUp())
                    squaresAvailable.Add(square.pos);
            }
            for (int i = 0; i < count; i++)
            {
                int index = squaresAvailable.GetRandomIndex();
                int2 squarePos = squaresAvailable[index];
                squaresAvailable.RemoveAt(index);
                squares[i] = _squares[squarePos.x, squarePos.y];
            }
            SquaresLightTween(squares);
        }

        void Start()
        {
            // TODO: remove speed vector, tween as TrianglesBackground
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

            OnColorPaletteChanged(0);
            ColorPalette.OnPaletteChanged += OnColorPaletteChanged;
            _initialized = true;
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
            if (manualLightUp || _squareLightUpDelay > 0) return;

            LightUpSquares(SQUARE_LIGHT_UP_COUNT);
            _squareLightUpDelay = SQUARE_LIGHT_UP_DELAY;
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
        Vector3 GetSquarePos(int x, int y)
        {
            return new Vector3(SQUARE_START_X + SQUARES_DISTANCE * x, SQUARE_START_Y + SQUARES_DISTANCE * y);
        }

        void OnColorPaletteChanged(int colorsChanged)
        {
            _lightColor = ColorPalette.C2.ColorCur.WithAlpha(0.75f);
            _defaultColor = ColorPalette.C4.ColorCur.WithAlpha(0.75f);

            for (int y = 0; y < GRID_SIZE_Y; y++)
            {
                for (int x = 0; x < GRID_SIZE_X; x++)
                    _squares[x, y].renderer.color = _defaultColor;
            }

            for (int i = 0; i < TWEENS_MAX; i++)
            {
                Tween tween = _tweens[i];
                if (tween.IsActive())
                    _tweens[i].onUpdate();
            }
        }
        void UpdateVisibleSquares()
        {
            _visibleSquareRangeX = GetVisibleSquaresRangeX();
            _visibleSquareRangeY = GetVisibleSquaresRangeY();
            int visibleLengthX = _visibleSquareRangeX.y - _visibleSquareRangeX.x;
            int visibleLengthY = _visibleSquareRangeY.y - _visibleSquareRangeY.x;
            int i = 0;

            _squaresVisible = new Square[visibleLengthX * visibleLengthY];
            for (int x = _visibleSquareRangeX.x; x < _visibleSquareRangeX.y; x++)
            {
                for (int y = _visibleSquareRangeY.x; y < _visibleSquareRangeY.y; y++)
                    _squaresVisible[i++] = _squares[x, y];
            }
        }
        void CreateSquares()
        {
            _squares = new Square[GRID_SIZE_X, GRID_SIZE_Y];
            for (int y = 0; y < GRID_SIZE_Y; y++)
            {
                for (int x = 0; x < GRID_SIZE_X; x++)
                {
                    GameObject instance = Instantiate(_squarePrefab, _gameObject.transform);
                    SpriteRenderer renderer = instance.GetComponent<SpriteRenderer>();

                    instance.name = $"Square [{x + 1}, {y + 1}]";
                    instance.transform.localPosition = GetSquarePos(x, y);
                    _squares[x, y] = new Square(renderer, new int2(x, y));
                }
            }
        }
        void SwitchVisibleSquares()
        {
            // switches visible squares positions between START_POS and END_POS
            // (used to keep squares animations)
            const int HALF_SIZE_X = GRID_SIZE_X / 2;
            const int HALF_SIZE_Y = GRID_SIZE_Y / 2;

            Square[,] invisibles = new Square[HALF_SIZE_X, HALF_SIZE_Y];
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
                    Square square = _squares[x, y];
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
                    Square square = invisibles[x, y];
                    square.transform.localPosition = GetSquarePos(newX, newY);
                    _squares[newX, newY] = square;
                }
            }
        }

        void SquaresLightTween(IEnumerable<Square> squares)
        {
            int index = _tweensFreeIndexes.Pop();
            foreach (Square square in squares)
            {
                square.SwitchLightUp();
                square.renderer.color = _lightColor;
            }
            Tween tween = DOVirtual.Float(0, 1, lightUpDuration, v => OnSquareTweenUpdate(squares, v)).OnComplete(() => OnSquareTweenComplete(squares, index));
            _tweens[index] = tween;
        }
        void OnSquareTweenComplete(IEnumerable<Square> squares, int tweenIndex)
        {
            _tweensFreeIndexes.Push(tweenIndex);
            foreach (Square square in squares)
                square.SwitchLightUp();
        }
        void OnSquareTweenUpdate(IEnumerable<Square> squares, float value)
        {
            Color color = Color.Lerp(_lightColor, _defaultColor, value);
            foreach (Square square in squares)
                square.renderer.color = color;
        }
    }
}
