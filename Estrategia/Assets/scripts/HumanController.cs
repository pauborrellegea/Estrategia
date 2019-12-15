using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HumanController : Player
{
    //MOUSE
    LayerMask layerMask;
    public GameObject circuloSeleccion;
    public GameObject circuloHighlight;

    private Unit selectedUnit;
    private int selectedX, selectedZ;

    private bool attacking = false;

    public Text attackButton, coinsText;

    public GameObject sceneCamera;
    private float minX = 10f;
    private float minZ = -4.5f;
    private float maxX = 33.5f;
    private float maxZ = 20f;

    private float cameraSpeed = 10f;

    private void Start()
    {
        layerMask = LayerMask.GetMask("Default");
        player = true;

        circuloSeleccion.SetActive(false);
        circuloHighlight.SetActive(false);
        
    }

    public override void AddCoins(int amount)
    {
        base.AddCoins(amount);
        coinsText.text = "Coins: " + coins;
    }

    public override void SubstractCoins(int amount)
    {
        base.SubstractCoins(amount);
        coinsText.text = "Coins: " + coins;
    }

    public override void Attack(Unit attacker, int x, int z)
    {
        if (gameController.turnOfPlayer() != player) return;

        if (gridController.CanAttack(x, z) && coins > attackCost)
        {
            if (x == otherBaseX && z == otherBaseZ)
            {
                gameController.AttackBase(attacker.ataque, player);
            }
            else
            {
                Unit attackedUnit = gridController.GetUnit(x, z);
                if (attackedUnit != null)
                {
                    attackedUnit.ReceiveDamage(attacker.ataque);
                    if (attackedUnit.IsDead())
                    {
                        gridController.RemoveUnit(attackedUnit, x, z);
                    }
                }
            }

            attacking = false;
            attackButton.text = "Attack";

            selectedUnit = null;
            circuloSeleccion.SetActive(false);

            gridController.SetMovement(null);
            gridController.SetAttack(null);

            SubstractCoins(attackCost);
            attacker.Attacked();

            GameObject att = Instantiate(attackPrefab, transform);
            att.GetComponent<AttackAnimation>().setObjectives(attacker.transform.position, new Vector3(x, 0, z));
        }
        else 
        {
            int attX = (int)attacker.transform.position.x;
            int attZ = (int)attacker.transform.position.z;
            if (attX == x && attZ == z)
            {
                selectedUnit = null;
                circuloSeleccion.SetActive(false);
                gridController.SetAttack(null);
                attacking = false;
                attackButton.text = "Attack";
            }
        }
    }

    public override void CreateUnit(UnitType type)
    {
        if (gameController.turnOfPlayer() != player) return;

        int cost = gameController.spawnableUnits[(int)type].coste;
        if (coins >= cost)
        {
            if (gridController.CanSpawnUnit(spawnX, spawnZ))
            {
                Unit newUnit = Instantiate(gameController.spawnableUnits[(int)type], new Vector3(spawnX, 0f, spawnZ), transform.rotation, transform) as Unit;
                newUnit.SetPlayer(player);

                gridController.AddUnit(newUnit, spawnX, spawnZ);
                SubstractCoins(cost);
            }
        }
    }

    public void CreateUnitBasic()
    {
        CreateUnit(UnitType.BASICA);
    }
    public void CreateUnitCannon()
    {
        CreateUnit(UnitType.CANNON);
    }
    public void CreateUnitExplorer()
    {
        CreateUnit(UnitType.EXPLORADOR);
    }
    public void CreateUnitLong()
    {
        CreateUnit(UnitType.LARGA);
    }
    public void CreateUnitTank()
    {
        CreateUnit(UnitType.TANQUE);
    }
    public void PrepareAttack()
    {
        if (!attacking)
        {
            if (selectedUnit != null && !selectedUnit.hasAttacked)
            {
                attackButton.text = "Move";
                attacking = true;
                gridController.SetMovement(null);
                gridController.SetAttack(selectedUnit);
            }
        }
        else
        {
            attackButton.text = "Attack";
            attacking = false;
            gridController.SetAttack(null);
            gridController.SetMovement(selectedUnit);
        }
    }

    public override void MoveUnit(Unit unit, int newX, int newZ)
    {
        if (gameController.turnOfPlayer() != player) return;

        if (gridController.CanMove(selectedX, selectedZ))
        {
            int moveCost = (int)gridController.MoveCost(newX, newZ);
            if (moveCost<=coins) //y le quedan movimientos
            {
                gridController.MoveUnit(selectedUnit, selectedX, selectedZ);

                selectedUnit = null;
                circuloSeleccion.SetActive(false);

                gridController.SetMovement(null);
                gridController.SetAttack(null);

                SubstractCoins(moveCost);
                unit.substractMoves(moveCost);
            }
        }
    }

    public override void resetEndTurn()
    {
        selectedUnit = null;
        gridController.SetMovement(null);
        gridController.SetAttack(null);
        attacking = false;
        attackButton.text = "Attack";
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            float newX = Mathf.Clamp(sceneCamera.transform.position.x + cameraSpeed * Time.deltaTime, minX, maxX);
            float newZ = Mathf.Clamp(sceneCamera.transform.position.z + cameraSpeed * Time.deltaTime, minZ, maxZ);
            sceneCamera.transform.position = new Vector3(newX, sceneCamera.transform.position.y, newZ);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            float newX = Mathf.Clamp(sceneCamera.transform.position.x - cameraSpeed * Time.deltaTime, minX, maxX);
            float newZ = Mathf.Clamp(sceneCamera.transform.position.z - cameraSpeed * Time.deltaTime, minZ, maxZ);
            sceneCamera.transform.position = new Vector3(newX, sceneCamera.transform.position.y, newZ);
        }
        if (Input.GetKey(KeyCode.UpArrow)) //arr der
        {
            float newX = Mathf.Clamp(sceneCamera.transform.position.x - cameraSpeed * Time.deltaTime, minX, maxX);
            float newZ = Mathf.Clamp(sceneCamera.transform.position.z + cameraSpeed * Time.deltaTime, minZ, maxZ);
            sceneCamera.transform.position = new Vector3(newX, sceneCamera.transform.position.y, newZ);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            float newX = Mathf.Clamp(sceneCamera.transform.position.x + cameraSpeed * Time.deltaTime, minX, maxX);
            float newZ = Mathf.Clamp(sceneCamera.transform.position.z - cameraSpeed * Time.deltaTime, minZ, maxZ);
            sceneCamera.transform.position = new Vector3(newX, sceneCamera.transform.position.y, newZ);
        }

        //-----------------------------Inputs temporales
        /*
        //Creacion de unidades
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CreateUnit(UnitType.BASICA);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            CreateUnit(UnitType.LARGA);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            CreateUnit(UnitType.EXPLORADOR);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            CreateUnit(UnitType.CANNON);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            CreateUnit(UnitType.TANQUE);
        }

        if (Input.GetKeyDown(KeyCode.T)) //terminar turno
        {
            EndTurn();
        }

        if (Input.GetKeyDown(KeyCode.Z)) //preparar ataque
        {
            PrepareAttack();
        }*/


        if (gameController.turnOfPlayer() == player)
        {
            if (Input.GetMouseButtonDown(0))
            {
                circuloHighlight.SetActive(false);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo))
                {
                    Transform hitTransform = hitInfo.transform;

                    //coordenadas de unidad o de casilla
                    selectedX = (int)hitTransform.position.x;
                    selectedZ = (int)hitTransform.position.z;

                    Unit selection = gridController.GetUnit(selectedX, selectedZ); //puede devolver null

                    if (!attacking)
                    {
                        //SELECCIONAR
                        if (selection != null && selection.player == player)
                        {
                            if (selectedUnit != selection)
                            {
                                selectedUnit = selection;
                                circuloSeleccion.SetActive(true);
                                circuloSeleccion.transform.position = selectedUnit.transform.position;

                                gridController.SetMovement(selectedUnit);
                            }
                            else
                            {
                                selectedUnit = null;
                                circuloSeleccion.SetActive(false);

                                gridController.SetMovement(selectedUnit);
                            }
                        }

                        //MOVER
                        else
                        {
                            if (selectedUnit != null)
                            {
                                MoveUnit(selectedUnit, selectedX, selectedZ);
                            }
                        }
                    }
                    else
                    {
                        //debe haber unidad seleccionada
                        Attack(selectedUnit, selectedX, selectedZ);
                    }
                    
                }

            }
            else
            {
                //POINTING

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo))
                {
                    Transform hitTransform = hitInfo.transform;

                    //coordenadas de unidad o de casilla
                    selectedX = (int)hitTransform.position.x;
                    selectedZ = (int)hitTransform.position.z;

                    bool point = false;
                    Unit u = gridController.GetUnit(selectedX, selectedZ);
                    if (u!=null && u.player)
                    {
                        point = true;
                    }
                    else if (attacking)
                    {
                        if (gridController.CanAttack(selectedX, selectedZ))
                        {
                            point = true;
                        }
                    }
                    else
                    {
                        if (gridController.CanMove(selectedX, selectedZ))
                        {
                            point = true;
                        }
                    }

                    circuloHighlight.transform.position = new Vector3(selectedX, 0, selectedZ);

                    //highlight
                    if (circuloSeleccion.transform.position != circuloHighlight.transform.position && point)
                        circuloHighlight.SetActive(true);
                    else
                        circuloHighlight.SetActive(false);
                }
                else
                {
                    circuloHighlight.SetActive(false);
                }
            }
        }
        else
        {
            circuloHighlight.SetActive(false);
            circuloSeleccion.SetActive(false);
            selectedUnit = null;
            gridController.SetMovement(null);
        }
        
    }
}
