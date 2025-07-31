using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nut : MonoBehaviour
{
    public MeshRenderer mesh_renderer;

    private Vector3 _origin_scale;
    public  ColorId ColorId { get; private set; }

    private Coroutine _coroutine;

    public void init(ColorAndMat data)
    {
        mesh_renderer.material = data.material;
        ColorId                = data.color_id;
        _origin_scale          = transform.localScale;
    }

    public void set_select(bool select)
    {
        transform.localScale = select ? _origin_scale * GameManager.Instance.config.nut_scale_multiplier : _origin_scale;
    }

    IEnumerator move_to_and_rotate(Vector3 startPos, Vector3 targetPos, float moveDuration, float rotationSpeed, float rotateDuration )
    {
        float elapsed    = 0f;
        float rotateTime = 0f;

        while (elapsed < moveDuration)
        {
            float t = elapsed / moveDuration;
            transform.position =  Vector3.Lerp(startPos, targetPos, t);
            elapsed            += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;

        while (rotateTime < rotateDuration)
        {
            rotateTime += Time.deltaTime;
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
            yield return null;
        }
    }

    // Example method to start the coroutine
    public void start_move_and_rotate(Vector3 startPos, Vector3 targetPos, float moveDuration, float rotationSpeed , float rotateDuration)
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            transform.eulerAngles = Vector3.zero;
            _coroutine            = null;
        }

        _coroutine = StartCoroutine(move_to_and_rotate(startPos, targetPos, moveDuration, rotationSpeed,rotateDuration));
    }
}