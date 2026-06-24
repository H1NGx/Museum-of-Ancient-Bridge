using UnityEngine;
using System.Collections;
using Assets.Scripts;
using UnityEngine.SceneManagement;


public class Game : MonoBehaviour
{
    void Start()
    {
        gameState = GameState.Start;

        ScalePieces();

        int index = Random.Range(0, Constants.MaxSize);
        go[index].SetActive(false);

        for (int i = 0; i < Constants.MaxColumns; i++)
        {
            for (int j = 0; j < Constants.MaxRows; j++)
            {
                if (go[i * Constants.MaxColumns + j].activeInHierarchy)
                {
                    Vector3 point = GetScreenCoordinatesFromVieport(i, j);
                    go[i * Constants.MaxColumns + j].transform.position = point;

                    Matrix[i, j] = new Piece();
                    Matrix[i, j].GameObject = go[i * Constants.MaxColumns + j];
                    Matrix[i, j].OriginalI = i; Matrix[i, j].OriginalJ = j;
                    if (Matrix[i, j].GameObject.GetComponent<BoxCollider2D>() == null)
                        Matrix[i, j].GameObject.AddComponent<BoxCollider2D>();
                }
                else
                {
                    Matrix[i, j] = null;
                }
            }
        }
    }

    private void Shuffle()
    {
        for (int i = 0; i < Constants.MaxColumns; i++)
        {
            for (int j = 0; j < Constants.MaxRows; j++)
            {
                if (Matrix[i, j] == null) continue;

                int random_i = Random.Range(0, Constants.MaxColumns);
                int random_j = Random.Range(0, Constants.MaxRows);
                Swap(i, j, random_i, random_j);
            }
        }
    }

    private void Swap(int i, int j, int random_i, int random_j)
    {
        Piece temp = Matrix[i, j];
        Matrix[i, j] = Matrix[random_i, random_j];
        Matrix[random_i, random_j] = temp;

        if (Matrix[i, j] != null)
            Matrix[i, j].GameObject.transform.position = GetScreenCoordinatesFromVieport(i, j);
        Matrix[random_i, random_j].GameObject.transform.position
            = GetScreenCoordinatesFromVieport(random_i, random_j);

        if (Matrix[i, j] != null)
        { Matrix[i, j].CurrentI = i; Matrix[i, j].CurrentJ = j; }
        Matrix[random_i, random_j].CurrentI = random_i;
        Matrix[random_i, random_j].CurrentJ = random_j;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("SampleScene");
            return;
        }

        switch (gameState)
        {
            case GameState.Start:
                if (Input.GetMouseButtonUp(0))
                {
                    Shuffle();
                    gameState = GameState.Playing;
                }
                break;
            case GameState.Playing:
                CheckPieceInput();
                break;
            case GameState.Animating:
                AnimateMovement(PieceToAnimate, Time.deltaTime);
                CheckIfAnimationEnded();
                break;
            case GameState.End:
                if (Input.GetMouseButtonUp(0))
                {   //reload
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
                break;
            default:
                break;
        }


    }
    void OnGUI()
    {
        switch (gameState)
        {
            case GameState.Start:
                GUI.Label(new Rect(0, 0, 100, 100), "点击开始游戏，按下esc退出");
                break;
            case GameState.Playing:
                break;
            case GameState.End:
                GUI.Label(new Rect(0, 0, 100, 100), "恭喜，点击重新开始游戏！");
                break;
            default:
                break;
        }
    }


    void CheckPieceInput()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null)
            {
                string name = hit.collider.gameObject.name;
                string[] parts = name.Split('-');
                int iPart = int.Parse(parts[1]);
                int jPart = int.Parse(parts[2]);

                int iFound = -1, jFound = -1;
                for (int i = 0; i < Constants.MaxColumns; i++)
                {
                    if (iFound != -1) break;
                    for (int j = 0; j < Constants.MaxRows; j++)
                    {
                        if (iFound != -1) break;
                        if (Matrix[i, j] == null) continue;
                        if (Matrix[i, j].OriginalI == iPart
                            && Matrix[i, j].OriginalJ == jPart)
                        {
                            iFound = i; jFound = j;
                        }
                    }
                }

                Piece foundPiece = Matrix[iFound, jFound];
                bool pieceFound = false;
                if (iFound > 0 && Matrix[iFound - 1, jFound] == null)
                {
                    pieceFound = true;
                    toAnimateI = iFound - 1; toAnimateJ = jFound;
                }
                else if (jFound > 0 && Matrix[iFound, jFound - 1] == null)
                {
                    pieceFound = true;
                    toAnimateI = iFound; toAnimateJ = jFound - 1;
                }
                else if (iFound < Constants.MaxColumns - 1 && Matrix[iFound + 1, jFound] == null)
                {
                    pieceFound = true;
                    toAnimateI = iFound + 1; toAnimateJ = jFound;
                }
                else if (jFound < Constants.MaxRows - 1 && Matrix[iFound, jFound + 1] == null)
                {
                    pieceFound = true;
                    toAnimateI = iFound; toAnimateJ = jFound + 1;
                }

                if(pieceFound)
                {
                    screenPositionToAnimate = GetScreenCoordinatesFromVieport(toAnimateI, toAnimateJ);
                    PieceToAnimate = Matrix[iFound, jFound];
                    gameState = GameState.Animating;
                }

            }

        }
    }


    private void AnimateMovement(Piece toMove,  float time)
    {
        toMove.GameObject.transform.position = Vector2.MoveTowards(toMove.GameObject.transform.position, 
          screenPositionToAnimate , time * AnimSpeed);
    }

    private void CheckIfAnimationEnded()
    {
        if(Vector2.Distance(PieceToAnimate.GameObject.transform.position, 
            screenPositionToAnimate) < 0.1f)
        {
            Swap(PieceToAnimate.CurrentI, PieceToAnimate.CurrentJ, toAnimateI, toAnimateJ);
            gameState = GameState.Playing;
            CheckForVictory();
        }
    }

    private void CheckForVictory()
    {
        for (int i = 0; i < Constants.MaxColumns; i++)
        {
            for (int j = 0; j < Constants.MaxRows; j++)
            {
                if (Matrix[i, j] == null) continue;
                if (Matrix[i, j].CurrentI != Matrix[i, j].OriginalI ||
                    Matrix[i, j].CurrentJ != Matrix[i, j].OriginalJ)
                    return; //at least one wrong piece, so we haven't won (yet!)
            }
        }
        //if we did not return, then we've won!
        gameState = GameState.End;
    }

    private void ScalePieces() {
        SpriteRenderer spriteRenderer = go[0].GetComponent<SpriteRenderer>();
        float screenHeight = Camera.main.orthographicSize * 2f;
        float screenWidth = screenHeight / Screen.height * Screen.width;
        float width = screenWidth / spriteRenderer.sprite.bounds.size.x / 4;
        float height = screenHeight / spriteRenderer.sprite.bounds.size.y / 4;
        for (int c = 0; c < go.Length; c++) {
            go[c].transform.localScale = new Vector3(width, height, 1f);
        }
    }

    private Vector3 GetScreenCoordinatesFromVieport(int i, int j)
    {
        Vector3 point = Camera.main.ViewportToWorldPoint(new Vector3(0.25f * j, 1 - 0.25f * i, 0));
        point.z = 0;
        return point;
    }

    Vector3 screenPositionToAnimate;
    private Piece PieceToAnimate;
    private int toAnimateI, toAnimateJ;

    public Piece[,] Matrix = new Piece[Constants.MaxColumns, Constants.MaxRows];
    private GameState gameState;
    public GameObject[] go;
    public float AnimSpeed = 10f;
}
