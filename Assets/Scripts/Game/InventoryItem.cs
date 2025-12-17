using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public TMP_Text countText;

    public int TotalSquareNumber;

    public Vector3 rotationPoint;

    public int quantity = 1;


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
    private bool Draggable = true;
    private Color color;
    

    void Start()
    {
        InventoryManager.Instance.RegisterItem(this);
    }

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
                /*
                Transform childTransform = transform.GetChild(0);
                Image childImage = childTransform.GetComponent<Image>();
                if (childImage != null)
                {
                    color = childImage.color;
                    childImage.color = Color.gray;
                }
                */
            }
            if (quantity > 0)
            {
                Draggable = true;

                //Az elem színét visszaállítjuk
                /*
                Transform childTransform = transform.GetChild(0);
                Image childImage = childTransform.GetComponent<Image>();
                if (childImage != null)
                {
                    childImage.color = color;
                }
                */
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Draggable)
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
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Draggable)
        {
            transform.position = Input.mousePosition;
        }
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
        countText.text = quantity.ToString();
    }
}
