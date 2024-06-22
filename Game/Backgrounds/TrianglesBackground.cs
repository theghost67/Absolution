using UnityEngine;

namespace Game.Backgrounds
{
    // works only with aspect ratio equal to 16:9
    // works only with non-movable camera (adjust squares BG object so it would be always on camera center)

    /// <summary>
    /// Класс для заднего фона "перемещение треугольников".
    /// </summary>
    public class TrianglesBackground : MonoBehaviour
    {
        const float START_POS_X = 3.2f;
        const float START_POS_Y = 1.8f;

        const float END_POS_X = -START_POS_X;
        const float END_POS_Y = -START_POS_Y;

        // readonly (set in Start())
        Vector3 _startPosScaled;
        Vector3 _speed;

        GameObject _gameObject;
        Transform _transform;
        // ------------------------

        void Start()
        {
            _startPosScaled = new Vector3(START_POS_X, START_POS_Y) * Global.PIXEL_SCALE;
            _speed = new Vector2(-16f, -9f);

            _gameObject = gameObject;
            _transform = _gameObject.transform;
            _transform.position = _startPosScaled;
        }
        void Update()
        {
            _transform.position += _speed * Time.deltaTime;
            if (_transform.position.x >= END_POS_X * Global.PIXEL_SCALE &&
                _transform.position.y <= END_POS_Y * Global.PIXEL_SCALE)
                _transform.position = _startPosScaled;
        }
    }
}
