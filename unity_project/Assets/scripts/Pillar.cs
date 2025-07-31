using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pillar : MonoBehaviour
{
    private int        _max_stack;
    private Stack<Nut> _nuts = new();

    private Coroutine _coroutine;
    public  bool      is_full() => _nuts.Count >= _max_stack;

    public void init(int maxStack, ConfigPillar configPillar)
    {
        _max_stack = maxStack;

        var gameManager = GameManager.Instance;
        var config      = gameManager.config;

        var nutPrefab = config.nut_prefab;
        if (!nutPrefab)
        {
            Debug.LogError("Nut prefab is not set in the GameManager config.");
            return;
        }

        if (configPillar.color_ids == null || configPillar.color_ids.Length == 0)
        {
            return;
        }


        foreach (var colorId in configPillar.color_ids)
        {
            var nut = Instantiate(nutPrefab);
            if (!config.try_get_data(colorId, out var colorData)) continue;
            nut.init(colorData);
            add_nut(nut);
        }
    }

    public void add_nut(Nut nut)
    {
        if (is_full())
        {
            Debug.LogWarning($"Cannot add nut: max stack of {_max_stack} reached.");
            return;
        }


        _nuts.Push(nut);

        if (nut.transform.parent == transform) return;
        nut.transform.SetParent(transform);
        var moveDuration = GameManager.Instance.config.move_duration;
        var rotateDuration = GameManager.Instance.config.rotate_duration;
        var rotateSpeed = GameManager.Instance.config.rotate_speed;
        var targetPosition = transform.position + new Vector3(0, get_offset_y_pos(_nuts.Count), 0);
        nut.start_move_and_rotate(nut.transform.position, targetPosition, moveDuration, rotateSpeed, rotateDuration);
        nut.transform.position = transform.position;
    }


    public int get_current_score()
    {
        if (_nuts.Count < 1) return 0;
        var list         = new List<Nut>(_nuts);
        var firstColorId = list[0].ColorId;
        var score        = 1;
        for (var i = 1; i < list.Count; i++)
        {
            var nut = list[i];
            if (nut.ColorId == firstColorId)
            {
                score++;
                continue;
            }

            return -1; // stop counting when color changes
        }

        return score < _max_stack ? -1 : score; // return -1 if the stack is not full
    }

    float get_offset_y_pos(int currentStackCount)
    {
        var heightNut = GameManager.Instance.config.nut_scale.y;
        var offsetY   = GameManager.Instance.config.offset_y_stack;
        return currentStackCount * heightNut + offsetY;
    }

    public bool try_pop_last_nut(out Nut nut)
    {
        return _nuts.TryPop(out nut);
    }

    IEnumerator shake_internal(float duration, float magnitude)
    {
        Vector3 originalPosition = transform.localPosition;
        float   elapsed          = 0f;

        while (elapsed < duration)
        {
            // Generate random offset within magnitude
            float xOffset = Random.Range(-magnitude, magnitude);
            float yOffset = Random.Range(-magnitude, magnitude);

            // Apply offset to the GameObject's position
            transform.localPosition = originalPosition + new Vector3(xOffset, yOffset, 0f);

            // Increment elapsed time
            elapsed += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Reset to original position
        transform.localPosition = originalPosition;
    }

    public void shake(float duration = 0.5f, float magnitude = 0.1f)
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }

        _coroutine = StartCoroutine(shake_internal(duration, magnitude));
    }

    void clear_coroutine()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }

    private void OnDestroy()
    {
        while (_nuts.TryPop(out var nut))
        {
            Destroy(nut.gameObject);
        }
    }
}