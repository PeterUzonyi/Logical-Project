using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //public Image image;
    public GameObject Element;
    public Text countText;

    public int TotalSquareNumber;

    public Vector3 rotationPoint;


    [HideInInspector]
    public Transform parentAfterDrag;
    [HideInInspector]
    public int count = 0;

    public static InventoryItem SelectedInventoryItem {  get; set; }
    public int ID;
    

    [HideInInspector]
    private Vector3[] originalPositions;
    private Vector3[] originalScales;
    private Transform[] children;
    private float scale;
    //private bool Draggable = true;
    

    public void Awake()
    {
        //Draggable = true;

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

        scale = 1 / children[0].localScale.x;
    }

    void Update()
    {
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
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        //image.raycastTarget = false;
        for (int i = 0; i < children.Length; i++)
        {
            children[i].localScale = Vector3.one;
            children[i].localPosition = originalPositions[i] * scale;
        }

        SelectedInventoryItem = this;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);
        //image.raycastTarget = true;
        for (int i = 0; i < children.Length; i++)
        {
            children[i].localScale = originalScales[i];
            children[i].localPosition = originalPositions[i];
        }
        GameEvents.CheckIfElementCanBePlaced();

        SelectedInventoryItem = null;
    }

    public void RefreshCount()
    {
        countText.text = count.ToString();
    }
}
