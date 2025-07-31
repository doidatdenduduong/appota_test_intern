using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Camera          main_camera;
    public GameConfig      config;
    public TextMeshProUGUI current_level_text;
    public TextMeshProUGUI current_level_text_info;
    public Button          btn_next_level;
    public Transform       next_level_panel;
    public Transform[]     pillar_containers;

    private int               _current_level_index = 0;
    private LevelDesignConfig _current_level_config;
    private List<Pillar>      _pillars = new();
    private List<Nut>         _nuts    = new();
    private Nut               _current_selected_nut;
    private Pillar            _current_pillar;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        load_level(ref _current_level_index);
        btn_next_level.onClick.AddListener(load_next_level);
    }

    void load_next_level()
    {
        reset_current_level();
        load_level(ref _current_level_index);
        next_level_panel.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (is_cursor_over_ui()) return;
        if (!Input.GetMouseButtonDown(0)) return;
        var ray    = main_camera.ScreenPointToRay(Input.mousePosition);
        var result = Physics.Raycast(ray, out var raycastHit, config.ray_distance, config.ray_mask);
        if (!result)
        {
            fallback_handle();
            return;
        }

        if (!raycastHit.collider.TryGetComponent<Pillar>(out var pillar))
        {
            fallback_handle();
            return;
        }

        if (_current_pillar == pillar && _current_selected_nut != null)
        {
            fallback_handle();
            return;
        }

        if (_current_pillar != null && pillar.is_full())
        {
            pillar.shake();
            fallback_handle();
            return;
        }

        swap_to(pillar);
        if (is_completed_this_level())
        {
            next_level_panel.gameObject.SetActive(true);
         
        }
    }

    /// <summary>
    /// set the current selected nut to the current pillar 
    /// </summary>
    void fallback_handle()
    {
        if (!_current_pillar) return;
        if (!_current_selected_nut) return;
        
        set_select_current_nut(false);
        _current_pillar.add_nut(_current_selected_nut);
        reset_current_selected();
    }

    void reset_current_selected()
    {
        _current_pillar       = null;
        _current_selected_nut = null;
    }

    void reset_current_level()
    {
        reset_current_selected();
        foreach (var pillar in _pillars)
        {
            Destroy(pillar.gameObject);
        }
    }

    bool is_completed_this_level()
    {
        var maxScore = _current_level_config.get_max_score();
        var score    = 0;
        for (int i = 0; i < _pillars.Count; i++)
        {
            var pillar = _pillars[i];
            score += pillar.get_current_score();
        }

        var completed = score == maxScore;

        Debug.Log($"completed: {completed}, score: {score}, maxScore: {maxScore}, maxStack: {_current_level_config.max_stack}");
        return completed;
    }

    void swap_to(Pillar newPillar)
    {
        _current_pillar = newPillar;

        if (_current_selected_nut != null) // transfer nut to new pillar
        {
            set_select_current_nut(false);
            _current_pillar.add_nut(_current_selected_nut);
            reset_current_selected();
        }
        else // active the nut of pillar
        {
            if (!_current_pillar.try_pop_last_nut(out var lastNut)) return;
            set_current_selected_nut(lastNut);
        }
    }

    void set_current_selected_nut(Nut nut)
    {
        set_select_current_nut(false);
        _current_selected_nut = nut;
        set_select_current_nut(true);
    }

    void set_select_current_nut(bool select)
    {
        if (_current_selected_nut != null) _current_selected_nut.set_select(select);
    }


    void setup_pillars()
    {
        var configPillars = _current_level_config.pillars;
        for (int i = 0; i < pillar_containers.Length; i++)
        {
            var pillarContainer = pillar_containers[i];
            var shouldActive    = i < configPillars.Length;
            pillarContainer.gameObject.SetActive(shouldActive);
            if (!shouldActive) continue;
            var pillarConfig = configPillars[i];
            setup_pillar(_current_level_config.max_stack, pillarConfig, pillarContainer);
        }
    }

    void setup_pillar(int maxStack, ConfigPillar pillar, Transform container)
    {
        var pillarObject = Instantiate(config.pillar_prefab, container);
        _pillars.Add(pillarObject);
        pillarObject.init(maxStack, pillar);
    }


    void load_level(ref int currentIndex)
    {
        if (currentIndex < 0 || currentIndex >= config.level_designs.Length)
        {
            Debug.LogError("Invalid level index: " + currentIndex);
            _current_level_config = null;
            return;
        }

        _current_level_config = config.level_designs[currentIndex];

        current_level_text.text      = $"Level {currentIndex + 1} / {config.level_designs.Length}";
        current_level_text_info.text = $"max stack per pillar: {_current_level_config.max_stack}";
        currentIndex++;

        setup_pillars();
    }

    bool is_cursor_over_ui()
    {
        var current = UnityEngine.EventSystems.EventSystem.current;
        if (!current)
        {
            Debug.LogError("EventSystem is not found.");
            return false;
        }

        return current.IsPointerOverGameObject();
    }
}