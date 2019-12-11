using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanController : Player
{
    //MOUSE
    LayerMask layerMask;
    public GameObject circuloSeleccion;
    public GameObject circuloHighlight;

    private Unit selectedUnit;
    private int selectedX, selectedZ;

    private bool attacking = false;

    private void Start()
    {
        layerMask = LayerMask.GetMask("Default");
        player = true;

        circuloSeleccion.SetActive(false);
        circuloHighlight.SetActive(false);
    }

    public override void Attack(Unit attacker, int x, int z)
    {
        if (gameController.turnOfPlayer() != player) return;

        if (gridController.CanAttack(selectedX, selectedZ))
        {
            if (selectedX == otherBaseX && selectedZ == otherBaseZ)
            {
                gameController.AttackBase(attacker.ataque, player);
            }
            else
            {
                Unit attackedUnit = gridController.GetUnit(x, z);
                if (attackedUnit != null)
                {
                    attackedUnit.ReceiveDamage(attacker.ataque);
                }
            }

            attacking = false;

            selectedUnit = null;
            circuloSeleccion.SetActive(false);

            gridController.SetMovement(null);
            gridController.SetAttack(null);

            //substract coins
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
                newUnit.player = player;

                gridController.AddUnit(newUnit, spawnX, spawnZ);
                SubstractCoins(cost);
            }
        }
    }

    public override void MoveUnit(Unit unit, int newX, int newZ)
    {
        if (gameController.turnOfPlayer() != player) return;

        if (gridController.CanMove(selectedX, selectedZ))
        {
            gridController.MoveUnit(selectedUnit, selectedX, selectedZ);

            selectedUnit = null;
            circuloSeleccion.SetActive(false);

            gridController.SetMovement(null);
            gridController.SetAttack(null);

            //substract coins
        }
    }

    public override void resetEndTurn()
    {
        selectedUnit = null;
        gridController.SetMovement(null);
        gridController.SetAttack(null);
        attacking = false;
    }

    private void Update()
    {
        //-----------------------------Inputs temporales

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
            if (!attacking)
            {
                if (selectedUnit != null)
                {
                    attacking = true;
                    gridController.SetMovement(null);
                    gridController.SetAttack(selectedUnit);
                }
            }
            else
            {
                attacking = false;
                gridController.SetAttack(null);
                gridController.SetMovement(selectedUnit);
            }
        }


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
                    
                    if (gridController.GetUnit(selectedX, selectedZ)!=null)
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
