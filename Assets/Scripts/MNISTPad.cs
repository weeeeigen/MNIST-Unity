using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Linq;
using Unity.Barracuda;

public class MNISTPad : MonoBehaviour
{
    const int width = 28;
    const int height = 28;

    Texture2D drawTexture;
    Color[] colors = new Color[width * height];
    float[] input = new float[width * height];

    public MeshRenderer m;
    public Text resultText;

    public NNModel modelSource;

    Model model;
    //IWorker worker;

    // Start is called before the first frame update
    void Start()
    {
        model = ModelLoader.Load(modelSource);

        drawTexture = new Texture2D(width, height);
        m.material.mainTexture = drawTexture;

        ResetColors();
    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                Draw(hit.textureCoord * width);
            }

            drawTexture.SetPixels(colors);
            drawTexture.Apply();
            m.material.mainTexture = drawTexture;

            //ReadBarracudaModel();
        }

        for (int i = 0; i < colors.Length; i++)
        {
            input[i] = colors[i].grayscale;
        }
    }

    public void Draw(Vector2 p)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if ((p - new Vector2(x, y)).magnitude < 1)
                {
                    colors.SetValue(Color.white, x + height * y);
                }
            }
        }
    }

    public void ResetColors()
    {
        colors = colors.Select((c) => Color.black).ToArray();
        drawTexture.SetPixels(colors);
        drawTexture.Apply();
        m.material.mainTexture = drawTexture;
    }

    public void SaveTextureToImage()
    {
        byte[] data = drawTexture.EncodeToJPG();
        string file = Application.dataPath + "/image.jpg";
        File.WriteAllBytes(file, data);
        //print("save image");
    }

    public void ReadBarracudaModel()
    {
        resultText.text = "Loading";
        var worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled ,model);
        resultText.text = "Loaded";

        var tensor = new Tensor(drawTexture, 1);
        worker.Execute(tensor);
        var result = worker.PeekOutput();
        var scores = result.AsFloats().ToList();
        float bestScore = 0;
        for (var i = 0; i < 10; i++)
        {
            if (scores[i] > bestScore)
            {
                bestScore = scores[i];
            }
        }
        worker.Dispose();
        resultText.text = scores.IndexOf(bestScore).ToString();
    }
}
