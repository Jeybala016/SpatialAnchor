using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Manages UI of anchor sample.
/// </summary>
[RequireComponent(typeof(JeyAnchorLoader))]
public class JeyUIManager : MonoBehaviour
{
    /// <summary>
    /// Anchor UI manager singleton instance
    /// </summary>
    public static JeyUIManager Instance;

    /// <summary>
    /// Anchor Mode switches between create and select
    /// </summary>
    public enum AnchorMode
    {
        Create,
        Select
    };

    [SerializeField, FormerlySerializedAs("createModeButton_")]
    private GameObject _createModeButton;

    [SerializeField, FormerlySerializedAs("selectModeButton_")]
    private GameObject _selectModeButton;

    [SerializeField, FormerlySerializedAs("trackedDevice_")]
    private Transform _trackedDevice;

    private Transform _raycastOrigin;

    private bool _drawRaycast = false;

    [SerializeField, FormerlySerializedAs("lineRendererHandler_")]
    private GameObject _lineRendererHandler;
    [SerializeField, FormerlySerializedAs("lineRenderer_")]
    private LineRenderer _lineRenderer;

    private Anchor _hoveredAnchor;

    private Anchor _selectedAnchor;

    private AnchorMode _mode = AnchorMode.Select;

    [SerializeField, FormerlySerializedAs("buttonList_")]
    private List<Button> _buttonList;

    int _buttonCount = 2;
    //public TMPro.TMP_Dropdown anchorDropdown;

    public JeyAnchorLoader loader;
    private int _menuIndex = 0;

    private Button _selectedButton;

    [SerializeField]
    private JeyAnchor _anchorPrefab;

    public JeyAnchor AnchorPrefab => _anchorPrefab;

    [SerializeField, FormerlySerializedAs("placementPreview_")]
    private GameObject _placementPreview;
    public GameObject _anchor1, _anchor2, _anchor3;

    [SerializeField, FormerlySerializedAs("anchorPlacementTransform_")]
    private Transform _anchorPlacementTransform;

    private delegate void PrimaryPressDelegate();

    private PrimaryPressDelegate _primaryPressDelegate;

    private bool _isFocused = true;

    #region Monobehaviour Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        _raycastOrigin = _trackedDevice;

        // Start in select mode
        _mode = AnchorMode.Select;
        StartSelectMode();

        _menuIndex = 0;
        _selectedButton = _buttonList[0];
        _selectedButton.OnSelect(null);

        _lineRenderer.startWidth = 0.005f;
        _lineRenderer.endWidth = 0.005f;
    }

    private void Update()
    {
        
        if (_selectedAnchor == null)
        {
            // Refocus menu
            _selectedButton.OnSelect(null);
            _isFocused = true;
        }

        HandleMenuNavigation();

        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
        {
            _primaryPressDelegate?.Invoke();
        }
    }

    #endregion // Monobehaviour Methods


    #region Menu UI Callbacks

    /// <summary>
    /// Create mode button pressed UI callback. Referenced by the Create button in the menu.
    /// </summary>
    public void OnCreateModeButtonPressed()
    {
        ToggleCreateMode();
        _createModeButton.SetActive(!_createModeButton.activeSelf);
        _selectModeButton.SetActive(!_selectModeButton.activeSelf);
        if (_selectModeButton.activeSelf)
        {
            for (int i = 2; i < _buttonList.Count; i++)
                _buttonList[i].gameObject.SetActive(true);
            _buttonCount = _buttonList.Count;
        }
        else
        {
            for (int i = 2; i < _buttonList.Count; i++)
                _buttonList[i].gameObject.SetActive(false);
            _buttonCount = 2;
        }
    }

    /// <summary>
    /// Load anchors button pressed UI callback. Referenced by the Load Anchors button in the menu.
    /// </summary>
    public void OnLoadAnchorsButtonPressed()
    {
        GetComponent<JeyAnchorLoader>().LoadAnchorsByUuid();
    }

    #endregion // Menu UI Callbacks  

    #region Mode Handling

    private void ToggleCreateMode()
    {
        if (_mode == AnchorMode.Select)
        {
            _mode = AnchorMode.Create;
            EndSelectMode();
            StartPlacementMode();
        }
        else
        {
            _mode = AnchorMode.Select;
            EndPlacementMode();
            StartSelectMode();
        }
    }

    private void StartPlacementMode()
    {
        ShowAnchorPreview();
        _primaryPressDelegate = PlaceAnchor;
    }

    private void EndPlacementMode()
    {
        HideAnchorPreview();
        _primaryPressDelegate = null;
    }

    private void StartSelectMode()
    {
        ShowRaycastLine();
        _primaryPressDelegate = SelectAnchor;
    }

    private void EndSelectMode()
    {
        HideRaycastLine();
        _primaryPressDelegate = null;
    }

    #endregion // Mode Handling


    #region Private Methods

    private void HandleMenuNavigation()
    {
        if (!_isFocused)
        {
            return;
        }

        if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickUp))
        {
            NavigateToIndexInMenu(false);
        }

        if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickDown))
        {
            NavigateToIndexInMenu(true);
        }

        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            _selectedButton.OnSubmit(null);
        }
    }

    private void NavigateToIndexInMenu(bool moveNext)
    {
        if (moveNext)
        {
            _menuIndex++;
            if (_menuIndex > _buttonCount - 1)
            {
                _menuIndex = 0;
            }
        }
        else
        {
            _menuIndex--;
            if (_menuIndex < 0)
            {
                _menuIndex = _buttonCount - 1;
            }
        }

        _selectedButton.OnDeselect(null);
        _selectedButton = _buttonList[_menuIndex];
        _selectedButton.OnSelect(null);
    }

    private void ShowAnchorPreview()
    {
        _placementPreview.SetActive(true);
    }

    private void HideAnchorPreview()
    {
        _placementPreview.SetActive(false);
    }

    private void PlaceAnchor()
    {
        Instantiate(_anchorPrefab, _anchorPlacementTransform.position, _anchorPlacementTransform.rotation);
    }

    private void ShowRaycastLine()
    {
        _drawRaycast = true;
        _lineRenderer.gameObject.SetActive(true);
        _lineRendererHandler.SetActive(true);
    }

    private void HideRaycastLine()
    {
        _drawRaycast = false;
        _lineRenderer.gameObject.SetActive(false);
        _lineRendererHandler.SetActive(false);
    }

    private void ControllerRaycast()
    {
        Ray ray = new Ray(_raycastOrigin.position, _raycastOrigin.TransformDirection(Vector3.forward));
        _lineRenderer.SetPosition(0, _raycastOrigin.position);
        _lineRenderer.SetPosition(1,
            _raycastOrigin.position + _raycastOrigin.TransformDirection(Vector3.forward) * 10f);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Anchor anchorObject = hit.collider.GetComponent<Anchor>();
            if (anchorObject != null)
            {
                _lineRenderer.SetPosition(1, hit.point);

                HoverAnchor(anchorObject);
                return;
            }
        }

        UnhoverAnchor();
    }

    private void HoverAnchor(Anchor anchor)
    {
        UnhoverAnchor();
        _hoveredAnchor = anchor;
        _hoveredAnchor.OnHoverStart();
    }

    private void UnhoverAnchor()
    {
        if (_hoveredAnchor == null)
        {
            return;
        }

        _hoveredAnchor.OnHoverEnd();
        _hoveredAnchor = null;
    }

    private void SelectAnchor()
    {
        if (_hoveredAnchor != null)
        {
            Debug.Log("HoverAnchor");
            if (_selectedAnchor != null)
            {
                // Deselect previous Anchor
                _selectedAnchor.OnSelect();
                _selectedAnchor = null;
            }

            // Select new Anchor
            _selectedAnchor = _hoveredAnchor;
            _selectedAnchor.OnSelect();

            // Defocus menu
            _selectedButton.OnDeselect(null);
            _isFocused = false;
        }
        else
        {
            Debug.Log("Not HoverAnchor");
            if (_selectedAnchor != null)
            {
                // Deselect previous Anchor
                _selectedAnchor.OnSelect();
                _selectedAnchor = null;

                // Refocus menu
                _selectedButton.OnSelect(null);
                _isFocused = true;
            }
        }
    }

    #endregion // Private Methods

    public void Anchor(int value)
    {
        _anchorPrefab = loader._Anchor[value].gameObject.GetComponent<JeyAnchor>();

        Debug.Log("Anchor "+value);
    }

}
