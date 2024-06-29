using DG.Tweening;
using UnityEngine;

namespace Game.Backgrounds
{
    /// <summary>
    /// Класс для заднего фона "перемещение треугольников".
    /// </summary>
    public class TrianglesBackground : MonoBehaviour
    {
        const float DURATION = 60;

        const float START_POS_X = 3.2f;
        const float START_POS_Y = 1.8f;

        const float END_POS_X = -2.47f;
        const float END_POS_Y = -1.23f;

        void Start()
        {
            Vector3 startPosScaled = new Vector3(START_POS_X, START_POS_Y) * Global.PIXEL_SCALE;
            Vector3 endPosScaled = new Vector3(END_POS_X, END_POS_Y) * Global.PIXEL_SCALE;

            transform.position = startPosScaled;
            transform.DOMove(endPosScaled, DURATION).SetLoops(-1, LoopType.Restart);
        }
    }
}
