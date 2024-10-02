using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ArrowController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    PointerEventData eventDataMemory;

    [SerializeField] float scaleMultiplayer = 1.4f;
    [SerializeField] float animationSpeed = 1f;

    public Action OnDragStarted;
    // returns pointer angle
    public Action<float> OnDrag;
    public Action OnDragFinished;

    Coroutine changeSizeAnimation = null;

    // При нажатии на стрелку, начинаем отслеживать касание пока оно удерживается
    public void OnPointerDown(PointerEventData eventData)
    {
        // Запоминаем касание, чтобы его отслеживать
        eventDataMemory = eventData;
        OnDragStarted?.Invoke();

        // Анимируем
        if (changeSizeAnimation != null)
            StopCoroutine(changeSizeAnimation);
        changeSizeAnimation = StartCoroutine(ChangeSize());

        // Следим за касанием
        StartCoroutine(CheckPosition());
    }

    // Уведомляем о точке касания и указываем на точку касания
    IEnumerator CheckPosition()
    {
        while (eventDataMemory != null)
        {
            Vector2 direction = new Vector2(
                eventDataMemory.position.x - transform.position.x,
                eventDataMemory.position.y - transform.position.y);

            // Вычисляем угол
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;

            // Приводим угол в диапазон [0, 360]
            if (angle < 0)
                angle += 360;

            // Вызываем событие OnDrag
            OnDrag?.Invoke(angle);

            // Применяем вращение
            transform.rotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }
    }

    // "Приподнимаем" стрелку пока ее тянут, потом опускаем
    IEnumerator ChangeSize()
    {
        while(eventDataMemory != null)
        {
            if((transform.localScale - 1.2f * Vector3.one).magnitude > 0.05f)
                transform.localScale = Vector3.Slerp(transform.localScale,
                                                     Vector3.one * scaleMultiplayer,
                                                     Time.deltaTime * animationSpeed);
            yield return null;
        }
        while ((transform.localScale - Vector3.one).magnitude > 0.05f)
        {
            transform.localScale = Vector3.Slerp(transform.localScale,Vector3.one, Time.deltaTime * animationSpeed);
            yield return null;
        }

        changeSizeAnimation = null;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        eventDataMemory = null;
        OnDragFinished?.Invoke();
    }
}
