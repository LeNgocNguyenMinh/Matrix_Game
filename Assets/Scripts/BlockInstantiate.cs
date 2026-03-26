using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Unity.Collections;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class BlockInstantiate : MonoBehaviour
{
    public Texture2D tex;
    public int width;
    public int heigh;
    public int[,] grid;
    public SpriteRenderer spr;
    public int eraseSize;
    public bool canSimulator;
    public float fallDelay;
    public float fallCounter;
    private void Start()
    {
        canSimulator = true;
        GridSetup();
        fallCounter = 0f;
        /* InvokeRepeating("SandFallSimulator", 0f, fallDelay); */
    }
    private void GridSetup()
    {
        grid = new int[width, heigh];

        tex = new Texture2D(width, heigh);
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
        HandleMouseHover();
        if (Input.GetKeyDown(KeyCode.T)) CreateTetrixBlock("T");
        if (Input.GetKeyDown(KeyCode.I)) CreateTetrixBlock("I");   
        fallCounter += Time.deltaTime;
        while (fallCounter >= fallDelay)
        {
            SandFallSimulator();
            
            // Trừ đi một khoảng delay, giữ lại phần dư (remainder) 
            // để cộng dồn cho frame kế tiếp. Đây là bí quyết của sự mượt mà.
            fallCounter -= fallDelay;

            // Giới hạn an toàn: Tránh việc vòng lặp chạy quá nhiều lần gây treo máy (Infinite loop)
            // Nếu máy quá lag, chúng ta chấp nhận bỏ qua một số bước vật lý.
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
                if(tex.GetPixel(x, y) == Color.black)
                {
                    if(tex.GetPixel(x, y - 1) == Color.white)
                    {
                        tex.SetPixel(x, y, Color.white);
                        tex.SetPixel(x, y - 1, Color.black);
                    }
                    else 
                    {
                        int direct = Random.Range(0,2) == 0 ? -1 : 1;
                        if((x + direct >= 0)  && (x + direct < width) && (tex.GetPixel(x + direct, y - 1) == Color.white))                        
                        {
                            tex.SetPixel(x, y, Color.white);
                            tex.SetPixel(x + direct, y - 1, Color.black);
                        }
                        else if((x - direct >= 0)  && (x - direct < width) && (tex.GetPixel(x - direct, y - 1) == Color.white)) 
                        {
                            tex.SetPixel(x, y, Color.white);
                            tex.SetPixel(x - direct, y - 1, Color.black);
                        }
                    }
                }
            }
        }
    }
    private void CreateTetrixBlock(string shapeType)
    {
        int [,] mask = null;
        switch(shapeType)
        {
            case "T":
                mask = new int[,] {
                    {1, 1, 1},
                    {0, 1, 0}
                };
                break;
            case "I":
                mask = new int[,]{
                    {0, 1, 0},
                    {0, 1, 0}
                }; 
                break;
            case "O":
                mask = new int[,]{
                    {1, 1},
                    {1, 1}
                };
                break;
            case "-":
                mask = new int[,]{
                    {1, 1, 1, 1}
                };
                break;
            case "L":
                mask = new int[,]{
                    {1, 0, 0, 0},
                    {1, 1, 1, 1}
                };
                break;
            case "z":
                mask = new int[,]{
                    {0, 1, 1},
                    {1, 1, 0}
                };
                break;
        }
        int cellSize = 5; // Mỗi ô vuông nhỏ của Tetris sẽ rộng 5x5 pixel cát
        int shapeWidth = mask.GetLength(1) * cellSize;
        int shapeHeight = mask.GetLength(0) * cellSize;

        int startX = width/2 - shapeWidth/2;
        int startY = heigh - 1;

        for (int row = 0; row < mask.GetLength(0); row++)
        {
            for (int col = 0; col < mask.GetLength(1); col++)
            {
                if (mask[row, col] == 1)
                {
                    for (int dy = 0; dy < cellSize; dy++)
                    {
                        for (int dx = 0; dx < cellSize; dx++)
                        {
                            int px = startX + (col * cellSize) + dx;
                            int py = startY - (row * cellSize) - dy;

                            if (px >= 0 && px < width && py >= 0 && py < heigh)
                            {
                                tex.SetPixel(px, py, Color.black);
                            }
                        }
                    }
                }
            }
        }
    }
}
