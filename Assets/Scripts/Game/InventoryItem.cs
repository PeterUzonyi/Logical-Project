using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// The element itself. Theese are in the inventories.
/// </summary>
public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// Quantity of the element in text form
    /// </summary>
    public TMP_Text countText;

    /// <summary>
    /// Number of squares of the element
    /// </summary>
    public int TotalSquareNumber;

    /// <summary>
    /// Rotation points for the rotations and flips
    /// </summary>
    public Vector3 rotationPoint;

    /// <summary>
    /// Quantity of the element
    /// </summary>
    public int quantity = 1;

    /// <summary>
    /// The level of the element
    /// </summary>
    public int level;

    /// <summary>
    /// The parent Object, needs for placing an element
    /// </summary>
    [HideInInspector]
    public Transform parentAfterDrag;

    /// <summary>
    /// Counts how many squares of the current element have been placed
    /// </summary>
    [HideInInspector]
    public int count = 0;

    /// <summary>
    /// The element that is being dragged
    /// </summary>
    public static InventoryItem SelectedInventoryItem {  get; set; }

    /// <summary>
    /// Uniq ID for every type of element (from 0 to 8)
    /// </summary>
    public int ID;
    
    /// <summary>
    /// Need for the dragging
    /// </summary>
    [HideInInspector]
    private Vector3[] originalPositions;
    private Vector3[] originalScales;
    private Transform[] children;
    private float scale;

    /// <summary>
    /// Whether the element is dragabble
    /// </summary>
    private bool Draggable = true;

    /// <summary>
    /// Cannot be dragged (needs in common reserve)
    /// </summary>
    public bool dragLocked = false;

    /// <summary>
    /// The color of the element (after placing down, the grid square's color must be the same as this element)
    /// </summary>
    private Color color;

    public Color ItemColor { get; private set; }

    public InventoryManager myInventoryManager;

    /// <summary>
    /// Whether the element is currently being dragged
    /// </summary>
    public static bool IsDragging = false;

    //Start is called before the first frame update
    void Start()
    {
        if (myInventoryManager != null)
        {
            myInventoryManager.RegisterItem(this);
        }

        if (transform.childCount > 0)
        {
            //Elem színének eltárolása
            Transform childTransform = transform.GetChild(0);
            Image childImage = childTransform.GetComponent<Image>();
            if (childImage != null)
            {
                color = childImage.color;
            }
        }
    }

    //Called when the script is loaded
    void Awake()
    {
        Draggable = true;
        RefreshCount();

        int count = transform.childCount;
        children = new Transform[count];
        originalPositions = new Vector3[count];
        originalScales = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            children[i] = transform.GetChild(i);
            originalPositions[i] = children[i].localPosition;
            originalScales[i] = children[i].localScale;
        }

        if (count > 0)
        {
            scale = 1 / children[0].localScale.x;
        }
        else
        {
            scale = 1f;
        }
    }

    //Update is called once per frame
    void Update()
    {
        if (countText == null || string.IsNullOrEmpty(countText.text))//Tesztelés miatt
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            //Rotation around
            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            //Rotation around backwards
            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //X tengelyen tükrözés
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            //Y tengelyen tükrözés
            Vector3 localScale = transform.localScale;
            localScale.y *= -1;
            transform.localScale = localScale;
        }

        if (int.Parse(countText.text) != quantity)
        {
            RefreshCount();
            if (quantity == 0)
            {
                Draggable = false;

                //Az elem színét szürkére állítjuk
                foreach (Transform child in transform)
                {
                    child.GetComponent<Image>().color = Color.gray;
                }
            }
            if (quantity > 0 && !dragLocked)
            {
                Draggable = true;

                //Az elem színét visszaállítjuk
                foreach (Transform child in transform)
                {
                    child.GetComponent<Image>().color = color;
                }                
            }
        }
    }

    /// <summary>
    /// Triggers when the element begins to be dragged
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!Draggable)
        {
            return;
        }

        IsDragging = true;

        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        for (int i = 0; i < children.Length; i++)
        {
            children[i].localScale = Vector3.one;
            children[i].localPosition = originalPositions[i] * scale;
        }

        SelectedInventoryItem = this;

        // Szín mentése itt, amíg biztosan elérhetõ
        Image img = GetComponentInChildren<Image>();
        if (img != null)
        {
            ItemColor = img.color;
        }            
    }

    /// <summary>
    /// Triggers when the element is being dragged
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        if (Draggable)
        {
            transform.position = Input.mousePosition;
        }
    }

    /// <summary>
    /// Triggers when the draggin is ended
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!Draggable)
        {
            return;
        }

        // Elõször küldd el az eseményt
        GameEvents.CheckIfElementCanBePlaced?.Invoke();
        IsDragging = false;

        transform.SetParent(parentAfterDrag);
        for (int i = 0; i < children.Length; i++)
        {
            children[i].localScale = originalScales[i];
            children[i].localPosition = originalPositions[i];
        }

        SelectedInventoryItem = null;
    }

    /// <summary>
    /// Whether the quantity of the element is changed, it changes too
    /// </summary>
    public void RefreshCount()
    {
        if (countText != null)
        {
            countText.text = quantity.ToString();
        }
    }

    /// <summary>
    /// Sets, whether the elements can be dragged or not. 
    /// If an element's quantity is 0, then it cannot be dragged
    /// </summary>
    /// <param name="value"></param>
    public void SetDraggable(bool value)
    {
        Draggable = value;
        dragLocked = !value;
    }
}
