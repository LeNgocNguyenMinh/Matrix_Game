using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BlockInstantiate : MonoBehaviour
{
    [Header("---General Setting---")]
    [SerializeField]private SpriteRenderer spr;
    private Texture2D tex;
    [SerializeField]private int width;
    [SerializeField]private int heigh;
    public int[,] grid;
    [Header("---List Of Block---")]
    private Dictionary <string, int[,]> maskList;
    private string[] listShape;
    [Header("---Sand Element---")]
    [SerializeField]private float sandFriction;
    [SerializeField]private Color[] colorList;
    [Header("---Simulator Setting")]
    public bool canSimulator;
    [SerializeField]private float fallDelay;
    [SerializeField]private float blockSpawnDelay;
    private float spawnCounter;
    private float fallCounter;
    private bool canSpawn = true;
    private void Awake()
    {
        //Shape name
        listShape = new string[]{"T", "I", "O", "-", "L", "Z"};
        //Create shape 
        maskList = new Dictionary<string, int[,]>();
        maskList["T"] = new int[,] {
            {1, 1, 1}, 
            {0, 1, 0}};
        maskList["I"] = new int[,] { 
            {1}, {1}, 
            {1}, {1}}; // Dạng thanh đứng
        maskList["O"] = new int[,] { 
            {1, 1}, 
            {1, 1}};
        maskList["-"] = new int[,] { 
            {1, 1, 1, 1}}; // Dạng thanh ngang
        maskList["L"] = new int[,] { 
            {1, 0}, 
            {1, 0}, 
            {1, 1}};
        maskList["Z"] = new int[,] { 
            {0, 1, 1}, 
            {1, 1, 0}};
        grid = new int[width, heigh];
        tex = new Texture2D(width, heigh);
    }
    private void Start()
    {
        canSimulator = true;
        GridSetup();
        fallCounter = 0f;
        spawnCounter = 0f;
    }
    //Make grid texture 2d
    private void GridSetup()
    {
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        for(int y = 0; y < heigh; y++)
        {  
            for(int x = 0; x < width; x++)
            {
                tex.SetPixel(x, y, Color.white);
            }
        }
        tex.Apply();
        spr.sprite = Sprite.Create(tex, new Rect(0, 0, width, heigh), Vector2.one * 0.5f, 16f);
    }
    private void Update()
    {
        /* HandleMouseHover(); */
        if(canSpawn)
        {
            if(spawnCounter >= blockSpawnDelay)
            {
                spawnCounter = 0f;
                int index = Random.Range(0, listShape.Length);
                CreateTetrixBlock(listShape[index]);
            }
            else{
                spawnCounter += Time.deltaTime;
            } 
        }
        
        fallCounter += Time.deltaTime;
        while (fallCounter >= fallDelay)
        {
            SandFallSimulator();
            fallCounter -= fallDelay;
            if (Time.deltaTime > 0.1f) {
                fallCounter = 0;
                break;
            }
        }
        tex.Apply();
    }
    private void HandleMouseHover()
    {
        if(!Input.GetMouseButton(0) && !Input.GetMouseButton(1))return;
        Vector3 mousePos = Input.mousePosition;
        // Lấy khoảng cách từ Camera đến mặt phẳng z=0
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z); 
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3 localPos = spr.transform.InverseTransformPoint(mouseWorldPos);
        //Caculate the cell[x,y] where mouse stay
        //move all to the right, so index x and y never < 0
        int px = Mathf.FloorToInt((localPos.x + width/2f/16f)*16f);
        int py = Mathf.FloorToInt((localPos.y + heigh/2f/16f)*16f);
        // get mouse pos
        
       

        // convert mousePos to position in the texture 2d
        

        if(px >= 0 && px < width && py >= 0 && py < heigh)
        {
            if (Input.GetMouseButton(0))
            {
                tex.SetPixel(px, py, Color.black);
            }
           
        }
    }
    private void SandFallSimulator()
    {
        //check from bottom to top, because if i do opposite, all sand will move in one frame
        for(int y = 1; y < heigh; y++)
        {  
            for(int x = 0; x < width; x++)
            {
                Color currentColor = tex.GetPixel(x, y);
                if(tex.GetPixel(x, y) != Color.white)
                {
                    if(tex.GetPixel(x, y - 1) == Color.white)
                    {
                        tex.SetPixel(x, y, Color.white);
                        tex.SetPixel(x, y - 1, currentColor);
                    }
                    else if (Random.value > sandFriction)
                    {
                        int direct = Random.Range(0,2) == 0 ? -1 : 1;
                        if((x + direct >= 0)  && (x + direct < width) && (tex.GetPixel(x + direct, y - 1) == Color.white))                        
                        {
                            tex.SetPixel(x, y, Color.white);
                            tex.SetPixel(x + direct, y - 1, currentColor);
                        }
                        else if((x - direct >= 0)  && (x - direct < width) && (tex.GetPixel(x - direct, y - 1) == Color.white)) 
                        {
                            tex.SetPixel(x, y, Color.white);
                            tex.SetPixel(x - direct, y - 1, currentColor);
                        }
                    }
                }
            }
        }
    }
    private void CreateTetrixBlock(string shapeType)
    {
        Color color = Color.black;
        int index = Random.Range(0, 3);
        switch (index)
        {
            case 0:
                color = Color.blue;
                break;
            case 1:
                color = Color.yellow;
                break;
            case 2:
                color = Color.red;
                break;
        }
        int cellSize = 5; // Mỗi ô vuông nhỏ của Tetris sẽ rộng 5x5 pixel cát
        int shapeWidth = maskList[shapeType].GetLength(1) * cellSize;
        int shapeHeight = maskList[shapeType].GetLength(0) * cellSize;

        int startX = width/2 - shapeWidth/2;
        int startY = heigh - 1;

        for (int row = 0; row < maskList[shapeType].GetLength(0); row++)
        {
            for (int col = 0; col < maskList[shapeType].GetLength(1); col++)
            {
                if (maskList[shapeType][row, col] == 1)
                {
                    for (int dy = 0; dy < cellSize; dy++)
                    {
                        for (int dx = 0; dx < cellSize; dx++)
                        {
                            int px = startX + (col * cellSize) + dx;
                            int py = startY - (row * cellSize) - dy;

                            if (px >= 0 && px < width && py >= 0 && py < heigh)
                            {
                                tex.SetPixel(px, py, color);
                            }
                        }
                    }
                }
            }
        }
    }
}
