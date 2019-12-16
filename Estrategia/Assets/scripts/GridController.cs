using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CielaSpike;

public class GridController : MonoBehaviour
{
    public MapDisplay mapMode;
    public GameObject quadDisplay;
    private Renderer displayRenderer;

    public Casilla grassCell, forestCell, mountainCell, towerCell;
    public Material playerBase, iaBase;

    GameController gameController;

    //perlin noise
    public float scale = 10f;
    public float offsetX, offsetY = 0f;
    public float fadeDist = 8f;
    public float cutGrass = 0.5f;
    public float cutForest = 0.35f;


    public int rows = 30; //mejor que sean iguales
    public int cols = 30;

    public int forestCost = 2;

    public Transform scene;

    private List<Unit> iaUnits;
    private List<Unit> playerUnits;

    //Grids globales
    private Casilla[,] gridCells;
    private Unit[,] gridUnits;

    private SingleMove[,] movementCells; //Destacadas y solo permiten moverse en ese rango
    private bool[,] attackCells;

    public int[,] playerVisibility; //numerado de 0 a N (N es la cantidad de unidades que pueden ver esa casilla)


    //RAW MAPS
    public Casilla[,] terrainSeen;
    public int[,] iaVisibility;
    public Unit[,] unitsSeen;

    public float maxAtackInf = 0f;

    //mapas de influencia
    public float[,] baseInfluence; //cercania a la base
    public float[,] attackInfluence; //puntos que se pueden atacar en ese turno
    public float[,] otherSeenAttackInfluence;
    public float[,] explorationInfluence;
    public float[,] terrainMobility;



    //parametros de importancia
    public float baseImportance;
    public float attackImportance;
    public float otherAttackImportance;
    public float explorationImportance;
    public float mobilityImportance;


    void Awake()
    {
        displayRenderer = quadDisplay.GetComponent<Renderer>();

        gameController = GetComponent<GameController>();

        iaUnits = new List<Unit>();
        playerUnits = new List<Unit>();

        gridCells = new Casilla[rows, cols];
        gridUnits = new Unit[rows, cols];
        movementCells = new SingleMove[rows, cols];
        attackCells = new bool[rows, cols];
        playerVisibility = new int[rows, cols];

        terrainSeen = new Casilla[rows, cols];
        iaVisibility = new int[rows, cols];
        unitsSeen = new Unit[rows, cols];

        baseInfluence = new float[rows, cols];
        attackInfluence = new float[rows, cols];
        otherSeenAttackInfluence = new float[rows, cols];
        explorationInfluence = new float[rows, cols];
        terrainMobility = new float[rows, cols];

        //parametros aleatorios
        baseImportance = Random.Range(7f, 15f);
        attackImportance = Random.Range(1f, 10f);
        otherAttackImportance = Random.Range(1f, 10f);
        explorationImportance = Random.Range(1f, 10f);
        mobilityImportance = Random.Range(1f, 10f);


        GenerateMap();
    }


    private void Update()
    {
        displayRenderer.material.mainTexture = GetTextureFromMap(mapMode);
    }

    //con otras funciones se pueden crear mapas de influencia o 
    //hacer busqueda de profundidad para comprobar la visibilidad del jugador Y del enemigo (por separado)
    //tambien se puede hacer un pathfiding sencillo con esa bfs

    public void UpdateUnitInfluences()
    {
        this.StartCoroutineAsync(UpdateIASeenUnits());
        GenerateIAAttackInfluence();
        this.StartCoroutineAsync(GenerateOtherAttackInfluence());
        this.StartCoroutineAsync(GenerateExplorationInfluence());
    }

    public bool CanSpawnUnit(int x, int z)
    {
        return gridUnits[x, z] == null;
    }
    public bool CanMove(int x, int z)
    {
        return movementCells[x, z]!=null;
    }

    public float MoveCost(int x, int z) //no deberia poder entrar con nulo
    {
        return movementCells[x, z].cost;
    }

    public bool CanAttack(int x, int z)
    {
        return attackCells[x, z];
    }

    public bool IA_CanAttack(Unit attacker, int oX, int oZ)
    {
        int atX = (int)attacker.transform.position.x;
        int atZ = (int)attacker.transform.position.z;

        return Mathf.Abs(atX - oX) + Mathf.Abs(atZ - oZ) <= attacker.rangoDeAtaque;
    }

    public void AddUnit(Unit unit, int x, int z)
    {
        gridUnits[x, z] = unit;
        AddVisibility(x, z, unit.vision, true, unit.player);

        if (unit.player)
        {
            playerUnits.Add(unit);
        }
        else
        {
            iaUnits.Add(unit);
        }

        UpdateFog();
        if (!gameController.turnOfPlayer())
            UpdateUnitInfluences();
    }

    public void RemoveUnit(Unit unit, int x, int z)
    {
        gridUnits[x, z] = null;
        unitsSeen[x, z] = null;
        AddVisibility(x, z, unit.vision, false, unit.player);
        if (unit.player)
        {
            playerUnits.Remove(unit);
        }
        else
        {
            iaUnits.Remove(unit);
            if (!gameController.turnOfPlayer())
                UpdateUnitInfluences();
        }
        Destroy(unit.gameObject);
    }

    public void AddIAUnit(Unit unit)
    {
        iaUnits.Add(unit);
    }

    public Unit GetUnit(int x, int z)
    {
        return gridUnits[x, z];
    }

    public Casilla GetCell(int x, int z)
    {
        return gridCells[x, z];
    }

    public void MoveUnit(Unit unit, int toX, int toZ)
    {
        //Esto deberia tener en cuenta mas cosas, y moverse suavemente
        

        //si se puede mover se mueve
        
        if (gridUnits[toX, toZ] == null)
        {
            int x = (int)unit.transform.position.x;
            int z = (int)unit.transform.position.z;
            gridUnits[x, z] = null;

            //eliminar visibilidad
            AddVisibility(x, z, unit.vision, false, unit.player);
            unit.transform.position = new Vector3(toX, 0, toZ);
            gridUnits[toX, toZ] = unit;
            //poner en la posicion nueva
            AddVisibility(toX, toZ, unit.vision, true, unit.player);

            UpdateFog();
            if (!gameController.turnOfPlayer())
                UpdateUnitInfluences();
        }
    }

    private void AddVisibility(int x, int z, int range, bool sum, bool player) //PROBLEMA: se ve visibilidad cuadriculada (puede ser la intencion pero no se) 
    {
        bool terrainAdded = false;

        int v = sum ? 1 : -1;
        bool[,] visitedCells = new bool[rows, cols];

        Queue<int[]> queue = new Queue<int[]>();

        visitedCells[x, z] = true;
        queue.Enqueue(new int[3]{x, z, range});
        
        while (queue.Count > 0)
        {
            int[] cell = queue.Dequeue();
            x = cell[0];
            z = cell[1];
            range = cell[2];

            if (player)
                playerVisibility[x, z] += v; //suma o resta
            else
            {
                iaVisibility[x, z] += v;
                if (terrainSeen[x, z] == null)
                {
                    terrainSeen[x, z] = gridCells[x, z];
                    terrainAdded = true;
                }
            }
                

            if (x > 0)
            {
                if (!visitedCells[x - 1, z] && range>0)
                {
                    visitedCells[x - 1, z] = true;
                    queue.Enqueue(new int[3] { x - 1, z, range - 1 });
                }
            }
            if (x < rows-1)
            {
                if (!visitedCells[x + 1, z] && range > 0)
                {
                    visitedCells[x + 1, z] = true;
                    queue.Enqueue(new int[3] { x + 1, z, range - 1 });
                }
            }
            if (z > 0)
            {
                if (!visitedCells[x, z - 1] && range > 0)
                {
                    visitedCells[x, z - 1] = true;
                    queue.Enqueue(new int[3] { x, z - 1, range - 1 });
                }
            }
            if (z < cols-1)
            {
                if (!visitedCells[x, z + 1] && range > 0)
                {
                    visitedCells[x, z + 1] = true;
                    queue.Enqueue(new int[3] { x, z + 1, range - 1 });
                }
            }
        }

        if (terrainAdded)
        {
            UpdateTerrainInfluence();
        }
    }

    private void UpdateFog()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (playerVisibility[r,c] > 0)
                {
                    //Lo ve
                    gridCells[r, c].EnableFog(false);
                } else
                {
                    //No lo ve ninguna unidad
                    gridCells[r, c].EnableFog(true);
                }
            }
        }
    }

    public void SelectedMovement(Unit unit)
    {
        SetMovement(unit, ref movementCells);
    }


    public void SetMovement(Unit unit, ref SingleMove[,] matrix)
    {
        matrix = new SingleMove[rows, cols]; //reset
        if (unit != null)
        {
            int x = (int)unit.transform.position.x;
            int z = (int)unit.transform.position.z;
            int maxCost = unit.remainingMoves;
            int range = 0;

            Queue<int[]> queue = new Queue<int[]>();

            movementCells[x, z] = new SingleMove(0, null, x, z);
            queue.Enqueue(new int[3] { x, z, range });

            while (queue.Count > 0)
            {
                int[] cell = queue.Dequeue();
                x = cell[0];
                z = cell[1];
                range = cell[2];
                
                if (x > 0)
                {
                    if (!((x-1==0 && z == cols-1) || (x-1 == rows-1 && z == 0)))
                        if (gridCells[x - 1, z].isGrass())
                        {
                            if (matrix[x - 1, z] == null && range < maxCost && gridUnits[x - 1, z] == null)
                            {
                                matrix[x - 1, z] = new SingleMove(range + 1, movementCells[x, z], x - 1, z);
                                queue.Enqueue(new int[3] { x - 1, z, range + 1 });
                            }
                            else if (movementCells[x - 1, z] != null && matrix[x - 1, z].cost > range + 1 && gridUnits[x - 1, z] == null)
                            {
                                matrix[x - 1, z] = new SingleMove(range + 1, movementCells[x, z], x - 1, z);
                                queue.Enqueue(new int[3] { x - 1, z, range + 1 });
                            }
                        }
                        else if (gridCells[x - 1, z].isForest() && range < maxCost - forestCost)
                        {
                            if (matrix[x - 1, z] == null && gridUnits[x - 1, z] == null)
                            {
                                matrix[x - 1, z] = new SingleMove(range + forestCost, movementCells[x, z], x - 1, z);
                                queue.Enqueue(new int[3] { x - 1, z, range + forestCost });
                            }
                            else if (movementCells[x - 1, z] != null && movementCells[x - 1, z].cost > range + forestCost && gridUnits[x - 1, z] == null)
                            {
                                matrix[x - 1, z] = new SingleMove(range + forestCost, movementCells[x, z], x - 1, z);
                                queue.Enqueue(new int[3] { x - 1, z, range + forestCost });
                            }
                        }
                }
                if (x < rows - 1)
                {
                    if (!((x+1 == 0 && z == cols - 1) || (x+1 == rows - 1 && z == 0)))
                        if (gridCells[x + 1, z].isGrass())
                        {
                            if (matrix[x + 1, z] == null && range < maxCost && gridUnits[x + 1, z] == null)
                            {
                                matrix[x + 1, z] = new SingleMove(range + 1, movementCells[x, z], x + 1, z);
                                queue.Enqueue(new int[3] { x + 1, z, range + 1 });
                            }
                            else if (matrix[x + 1, z] != null && matrix[x + 1, z].cost > range + 1 && gridUnits[x + 1, z] == null)
                            {
                                matrix[x + 1, z] = new SingleMove(range + 1, movementCells[x, z], x + 1, z);
                                queue.Enqueue(new int[3] { x + 1, z, range + 1 });
                            }
                        }
                        else if (gridCells[x + 1, z].isForest() && range < maxCost - forestCost)
                        {
                            if (matrix[x + 1, z] == null && gridUnits[x + 1, z] == null)
                            {
                                matrix[x + 1, z] = new SingleMove(range + forestCost, movementCells[x, z], x + 1, z);
                                queue.Enqueue(new int[3] { x + 1, z, range + forestCost });
                            }
                            else if (matrix[x + 1, z] != null && matrix[x + 1, z].cost > range + forestCost && gridUnits[x + 1, z] == null)
                            {
                                matrix[x + 1, z] = new SingleMove(range + forestCost, movementCells[x, z], x + 1, z);
                                queue.Enqueue(new int[3] { x + 1, z, range + forestCost });
                            }
                        }
                }
                if (z > 0)
                {
                    if (!((x == 0 && z-1 == cols - 1) || (x == rows - 1 && z-1 == 0)))
                        if (gridCells[x, z - 1].isGrass())
                        {
                            if (matrix[x, z - 1] == null && range < maxCost && gridUnits[x, z - 1] == null)
                            {
                                matrix[x, z - 1] = new SingleMove(range + 1, movementCells[x, z], x, z - 1);
                                queue.Enqueue(new int[3] { x, z - 1, range + 1 });
                            }
                            else if (matrix[x, z - 1] != null && matrix[x, z - 1].cost > range + 1 && gridUnits[x, z - 1] == null)
                            {
                                matrix[x, z - 1] = new SingleMove(range + 1, movementCells[x, z], x, z - 1);
                                queue.Enqueue(new int[3] { x, z - 1, range + 1 });
                            }
                        }
                        else if (gridCells[x, z - 1].isForest() && range < maxCost - forestCost)
                        {
                            if (matrix[x, z - 1] == null && gridUnits[x, z - 1] == null)
                            {
                                matrix[x, z - 1] = new SingleMove(range + forestCost, movementCells[x, z], x, z - 1);
                                queue.Enqueue(new int[3] { x, z - 1, range + forestCost });
                            }
                            else if (matrix[x, z - 1] != null && matrix[x, z - 1].cost > range + forestCost && gridUnits[x, z - 1] == null)
                            {
                                matrix[x, z - 1] = new SingleMove(range + forestCost, movementCells[x, z], x, z - 1);
                                queue.Enqueue(new int[3] { x, z - 1, range + forestCost });
                            }
                        }
                }
                if (z < cols - 1)
                {
                    if (!((x == 0 && z+1 == cols - 1) || (x == rows - 1 && z+1 == 0)))
                        if (gridCells[x, z + 1].isGrass())
                        {
                            if (matrix[x, z + 1] == null && range < maxCost && gridUnits[x, z + 1] == null)
                            {
                                matrix[x, z + 1] = new SingleMove(range + 1, movementCells[x, z], x, z + 1);
                                queue.Enqueue(new int[3] { x, z + 1, range + 1 });
                            }
                            else if (matrix[x, z + 1] != null && matrix[x, z + 1].cost > range + 1 && gridUnits[x, z + 1] == null)
                            {
                                matrix[x, z + 1] = new SingleMove(range + 1, movementCells[x, z], x, z + 1);
                                queue.Enqueue(new int[3] { x, z + 1, range + 1 });
                            }
                        }
                        else if (gridCells[x, z + 1].isForest() && range < maxCost - forestCost)
                        {
                            if (matrix[x, z + 1] == null && gridUnits[x, z + 1] == null)
                            {
                                matrix[x, z + 1] = new SingleMove(range + forestCost, movementCells[x, z], x, z + 1);
                                queue.Enqueue(new int[3] { x, z + 1, range + forestCost });
                            }
                            else if (matrix[x, z + 1] != null && matrix[x, z + 1].cost > range + forestCost && gridUnits[x, z + 1] == null)
                            {
                                matrix[x, z + 1] = new SingleMove(range + forestCost, movementCells[x, z], x, z + 1);
                                queue.Enqueue(new int[3] { x, z + 1, range + forestCost });
                            }
                        }
                }
            }
        }
        

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (movementCells[r, c]!=null)
                {
                    gridCells[r, c].EnableIndicator(true);
                    gridCells[r, c].setColor(false);
                }
                else
                {
                    gridCells[r, c].EnableIndicator(false);
                }
            }
        }

        //gridCells[0, 0].setColor(false);
    }

    public void SetAttack(Unit unit)
    {
        attackCells = new bool[rows, cols]; //reset
        if (unit != null)
        {
            int x = (int)unit.transform.position.x;
            int z = (int)unit.transform.position.z;
            int range = unit.rangoDeAtaque;

            Queue<int[]> queue = new Queue<int[]>();

            attackCells[x, z] = true;
            queue.Enqueue(new int[3] { x, z, range });

            while (queue.Count > 0)
            {
                int[] cell = queue.Dequeue();
                x = cell[0];
                z = cell[1];
                range = cell[2];


                if (x > 0)
                {
                    if (!attackCells[x - 1, z] && range > 0)
                    {
                        if (gridUnits[x - 1, z]==null || !gridUnits[x - 1, z].player)
                        {
                            attackCells[x - 1, z] = true;
                        }
                        queue.Enqueue(new int[3] { x - 1, z, range - 1 });
                    }
                }
                if (x < rows - 1)
                {
                    if (!attackCells[x + 1, z] && range > 0)
                    {
                        if (gridUnits[x + 1, z] == null || !gridUnits[x + 1, z].player)
                        {
                            attackCells[x + 1, z] = true;
                        }
                        queue.Enqueue(new int[3] { x + 1, z, range - 1 });
                    }
                }
                if (z > 0)
                {
                    if (!attackCells[x, z - 1] && range > 0)
                    {
                        if (gridUnits[x, z - 1] == null || !gridUnits[x, z - 1].player)
                        {
                            attackCells[x, z - 1] = true;
                        }
                        queue.Enqueue(new int[3] { x, z - 1, range - 1 });
                    }
                }
                if (z < cols - 1)
                {
                    if (!attackCells[x, z + 1] && range > 0)
                    {
                        if (gridUnits[x, z + 1] == null || !gridUnits[x, z + 1].player)
                        {
                            attackCells[x, z + 1] = true;
                        }
                        queue.Enqueue(new int[3] { x, z + 1, range - 1 });
                    }
                }
            }
            x = (int)unit.transform.position.x;
            z = (int)unit.transform.position.z;
            attackCells[x, z] = false;
            
            attackCells[rows - 2, 1] = false;
        }
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (attackCells[r, c])
                {
                    gridCells[r, c].EnableIndicator(true);
                    gridCells[r, c].setColor(true);
                }
                else
                {
                    gridCells[r, c].EnableIndicator(false);
                }
            }
        }
        
    }

    public void GenerateMap()
    {
        offsetX = Random.Range(0f, 99999f);
        offsetY = Random.Range(0f, 99999f);

        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                

                Casilla newCell;
                if (x==1 && z == cols - 2)
                {
                    //ia
                    newCell = Instantiate(towerCell, new Vector3(x, 0f, z), transform.rotation, scene) as Casilla;
                    setBaseColor(newCell, false);
                    gridCells[x, z] = newCell;
                    continue;
                } else if (x == rows - 2 && z == 1)
                {
                    //player base
                    newCell = Instantiate(towerCell, new Vector3(x, 0f, z), transform.rotation, scene) as Casilla;
                    setBaseColor(newCell, true);
                    gridCells[x, z] = newCell;
                    continue;
                } else
                {
                    float sample = CalculateCell(x, z);
                    if (sample > cutGrass)
                    {
                        newCell = Instantiate(grassCell, new Vector3(x, 0f, z), transform.rotation, scene) as Casilla;
                    }
                    else if (sample > cutForest)
                    {
                        newCell = Instantiate(forestCell, new Vector3(x, 0f, z), transform.rotation, scene) as Casilla;
                    }
                    else
                    {
                        newCell = Instantiate(mountainCell, new Vector3(x, 0f, z), transform.rotation, scene) as Casilla;
                    }
                    gridCells[x, z] = newCell;
                }
            }
        }
    }

    float CalculateCell(int x, int y)
    {
        float dist = Mathf.Min(x, y, rows - x - 1, cols - y - 1);

        float fade = 1f;

        if (dist < rows / fadeDist)
        {
            fade = (1+fadeDist) * dist / rows;
        }

        float xCoord = (float)x / rows * scale + offsetX;
        float yCoord = (float)y / cols * scale + offsetY;

        float sample = 1f - Mathf.PerlinNoise(xCoord, yCoord) * fade;

        return sample;

    }

    void setBaseColor(Casilla tower, bool player)
    {
        Material color = player ? playerBase : iaBase;

        tower.transform.GetChild(2).GetChild(0).GetComponent<Renderer>().material = color;
        tower.transform.GetChild(2).GetChild(1).GetComponent<Renderer>().material = color;
    }

    public void resetAllUnits()
    {
        for(int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (gridUnits[r, c] != null)
                {
                    gridUnits[r, c].ResetTurn();
                }
            }
        }
    }


    public float TacticMoveCost(int x, int z)
    {
        return baseImportance * baseInfluence[x, z] + attackImportance * attackInfluence[x, z] +
            otherAttackImportance * otherSeenAttackInfluence[x, z] + explorationImportance * explorationInfluence[x, z] +
            mobilityImportance * terrainMobility[x, z];
    }


    // MAPAS DE INFLUENCIA
    //*************************************************************
    public void GenerateBaseInfluence()
    {
        this.StartCoroutineAsync(NewBaseInfluence());
    }
    private IEnumerator NewBaseInfluence()
    {
        int spX = gameController.ia.otherBaseX;
        int spZ = gameController.ia.otherBaseZ;

        float max = -Mathf.Infinity;
        float min = Mathf.Infinity;

        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                float inf = Mathf.Abs(x - spX) + Mathf.Abs(z - spZ);
                baseInfluence[x, z] = inf;
                if (inf < min) min = inf;
                if (inf > max) max = inf;
            }
        }

        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                baseInfluence[x, z] = (baseInfluence[x, z]-min)/(max-min);
            }
        }

        yield return Ninja.JumpToUnity;
    }

    private IEnumerator UpdateIASeenUnits()
    {
        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                if (iaVisibility[x, z]>0 && gridUnits[x, z]!=null && gridUnits[x, z].player)
                {
                    unitsSeen[x, z] = gridUnits[x, z];
                } else
                {
                    unitsSeen[x, z] = null;
                }
            }
        }

        yield return Ninja.JumpToUnity;
    }

    public void GenerateIAAttackInfluence()
    {
        this.StartCoroutineAsync(ThreadAttackInfluence());
    }

    private IEnumerator ThreadAttackInfluence()
    {
        attackInfluence = new float[rows, cols];

        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                if (gridUnits[x, z]!=null && !gridUnits[x, z].player)
                {
                    Unit unit = gridUnits[x, z];
                    AddAttackInfluence(ref attackInfluence, unit, x, z, unit.rangoDeAtaque + unit.remainingMoves);
                }
            }
        }

        float max = maxAtackInf;

        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                attackInfluence[x, z] = 1f - attackInfluence[x, z] / max;
            }
        }

        yield return Ninja.JumpToUnity;
    }

    private IEnumerator GenerateOtherAttackInfluence()
    {
        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                if (unitsSeen[x, z] != null)
                {
                    AddAttackInfluence(ref otherSeenAttackInfluence, unitsSeen[x, z], x, z, unitsSeen[x, z].rangoDeAtaque);
                }
            }
        }

        float max = 10f; //maximo posible

        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                otherSeenAttackInfluence[x, z] = otherSeenAttackInfluence[x, z] / max;
            }
        }

        yield return Ninja.JumpToUnity;
    }


    private void AddAttackInfluence(ref float[,] matrixToEdit, Unit attacker, int posX, int posZ, int range)
    {
        for (int x = -range; x<=range; x++)
        {
            for (int z = -range+Mathf.Abs(x); z<=range- Mathf.Abs(x); z++)
            {
                if (posX + x >= 0 && posX + x < rows && posZ + z >= 0 && posZ + z < cols)
                {
                    matrixToEdit[posX + x, posZ + z] += attacker.ataque;
                    if (matrixToEdit[posX + x, posZ + z] > maxAtackInf)
                    {
                        maxAtackInf = matrixToEdit[posX + x, posZ + z];
                    }
                }
            }
        }
    }

    public void UpdateTerrainInfluence()
    {
        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                if (terrainSeen[x, z])
                    terrainMobility[x, z] = GaussianFilter5x5(x, z);
            }
        }
    }

    private IEnumerator GenerateExplorationInfluence()
    {
        explorationInfluence = new float[rows, cols];
        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                explorationInfluence[x, z] = GaussianExploration(x, z);
            }
        }

        float max = -Mathf.Infinity;
        float min = Mathf.Infinity;

        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                float inf = explorationInfluence[x, z];
                if (inf < min) min = inf;
                if (inf > max) max = inf;
            }
        }

        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                explorationInfluence[x, z] = (explorationInfluence[x, z] - min) / (max - min);
            }
        }

        yield return Ninja.JumpToUnity;
    }

    public float GaussianExploration(int ox, int oz)
    {
        float[] values = new float[] { 6f, 4f, 1f };

        float total = 0f;

        for (int x = -2; x <= 2; x++)
        {
            for (int z = -2; z <= 2; z++)
            {
                float mult = values[Mathf.Abs(x)] * values[Mathf.Abs(z)];
                if (ox + x >= 0 && ox + x < rows && oz + z >= 0 && oz + z < cols)
                {
                    if (iaVisibility[ox + x, oz + z] > 0)
                    {
                        total +=  mult;
                    }
                }
                else
                {
                    total += mult;
                }
            }
        }

        return total / 246f;
    }

    public float GaussianFilter5x5(int ox, int oz)
    {
        float[] values = new float[] { 6f, 4f, 1f };

        float total = 0f;

        for (int x = -2; x<=2; x++)
        {
            for (int z = -2; z <= 2; z++)
            {
                float mult = values[Mathf.Abs(x)] * values[Mathf.Abs(z)];
                if (ox + x >= 0 && ox + x < rows && oz + z >= 0 && oz + z < cols)
                {
                    Casilla cell = terrainSeen[ox+x, oz+z];
                    if (cell == null)
                    {
                        total += (0.5f*mult);
                    } else if (cell.type == Casilla.CellType.MOUNTAIN)
                    {
                        total += (1f*mult);
                    } else if (cell.type == Casilla.CellType.FOREST)
                    {
                        total += (0.3f*mult);
                    }
                } else
                {
                    total += (1f*mult);
                }
            }
        }

        return total / 246f;
    }


    Texture2D GetTextureFromMap(MapDisplay map)
    {
        if (map == MapDisplay.RAWCELLS) { return GenerateTexture(ref gridCells); }
        if (map == MapDisplay.RAWUNITS) { return GenerateTexture(ref gridUnits); }
        if (map == MapDisplay.IA_VISIBILITY) { return GenerateTexture(ref iaVisibility); }
        if (map == MapDisplay.PL_VISIBILITY) { return GenerateTexture(ref playerVisibility); }
        if (map == MapDisplay.PL_ATTACK) { return GenerateTexture(ref attackCells); }
        if (map == MapDisplay.BASE_INF) { return GenerateTexture(ref baseInfluence); }
        if (map == MapDisplay.SEEN_MAP_RAW) { return GenerateTexture(ref terrainSeen); }
        if (map == MapDisplay.SEEN_UNITS_RAW) { return GenerateTexture(ref unitsSeen); }
        if (map == MapDisplay.IA_ATT_INF) { return GenerateTexture(ref attackInfluence); }
        if (map == MapDisplay.OTHER_ATT_INF) { return GenerateTexture(ref otherSeenAttackInfluence); }
        if (map == MapDisplay.EXPLORATION_INF) { return GenerateTexture(ref explorationInfluence); }
        if (map == MapDisplay.TERRAIN_INF) { return GenerateTexture(ref terrainMobility); }


        return new Texture2D(rows, cols);

    }

    Texture2D GenerateTexture(ref Casilla[,] matrix)
    {
        Texture2D texture = new Texture2D(rows, cols);
        texture.filterMode = FilterMode.Point;

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                
                Color color = Color.white;
                if (matrix[x, y] == null) color = Color.magenta;
                else if (matrix[x, y].type == Casilla.CellType.FOREST)
                {
                    color = Color.grey;
                }
                else if (matrix[x, y].type == Casilla.CellType.MOUNTAIN)
                {
                    color = Color.black;
                }
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }
    Texture2D GenerateTexture(ref Unit[,] matrix)
    {
        Texture2D texture = new Texture2D(rows, cols);
        texture.filterMode = FilterMode.Point;

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                Color color = Color.black;
                if (matrix[x, y] != null)
                {
                    if (matrix[x, y].player)
                    {
                        color = Color.blue;
                    } else
                    {
                        color = Color.red;
                    }
                }
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }
    Texture2D GenerateTexture(ref bool[,] matrix)
    {
        Texture2D texture = new Texture2D(rows, cols);
        texture.filterMode = FilterMode.Point;

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                Color color = Color.black;
                if (matrix[x, y])
                {
                    color = Color.white;
                } else
                {
                    color = Color.black;
                }
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }

    Texture2D GenerateTexture(ref int[,] matrix) //visibilidad
    {
        Texture2D texture = new Texture2D(rows, cols);
        texture.filterMode = FilterMode.Point;

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                Color color = Color.black;
                if (matrix[x, y]>0)
                {
                    color = Color.white;
                }
                else
                {
                    color = Color.black;
                }
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }
    Texture2D GenerateTexture(ref float[,] matrix)
    {
        Texture2D texture = new Texture2D(rows, cols);
        texture.filterMode = FilterMode.Point;

        float max = -Mathf.Infinity;
        float min = Mathf.Infinity;

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                if (matrix[x, y] > max) max = matrix[x, y];
                if (matrix[x, y] < min) min = matrix[x, y];
            }
        }

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                if (max==min) texture.SetPixel(x, y, Color.black);
                else
                {
                    float sample = 1f - (matrix[x, y] - min) / (max-min);
                    texture.SetPixel(x, y, new Color(sample, sample, sample));
                }
            }
        }
        texture.Apply();
        return texture;
    }
}

public enum MapDisplay
{
    RAWCELLS, RAWUNITS, PL_ATTACK, PL_VISIBILITY, IA_VISIBILITY, BASE_INF, SEEN_MAP_RAW, SEEN_UNITS_RAW, IA_ATT_INF, OTHER_ATT_INF, EXPLORATION_INF, TERRAIN_INF
}


public class SingleMove
{
    public int x;
    public int z;
    public float cost;
    public SingleMove comesFrom;

    public SingleMove(float c, SingleMove d, int x, int z)
    {
        cost = c;
        comesFrom = d;
        this.x = x;
        this.z = z;
    }
}