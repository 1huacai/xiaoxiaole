using System;
using System.Collections.Generic;
using UnityEngine;

// selection state class
class SelectionState : StateBase
{
    public SelectionState(GameController controller) : base(controller)
    {
        // Do nothing
    }

    public override void Enter()
    {
        base.Enter();
        _controller._chainCnt = 0;
        _controller._firstSelected.IsSelected = true;
        _controller._firstSelected.ResetDragPos();
        if (_controller._secondSelected != null)
        {
            _controller._secondSelected = null;
        }
    }

    public override void OnBlockOperation(int row, int column, BlockOperation operation)
    {
        base.OnBlockOperation(row, column, operation);
        Debug.Log(_controller._boardType + " -- operation=" + operation);
        if (operation == BlockOperation.TouchDown || operation == BlockOperation.DragHalf)
        {
            if (row >= 0 && row < _controller._curRowCnt && column >= 0 && column < Config.columns)
            {
                var selectedBlock = _controller._blockMatrix[row, column];
                Debug.Log(_controller._boardType + "--- second select block[" + row + "," + column + "] -- " + (selectedBlock == null ? "is null" : "is not null"));
                if (selectedBlock == null)
                {
                    var first = _controller._firstSelected;
                    if (first.Row != row || column < 0 || column >= Config.columns)
                    {
                        _controller.ChangeToState(GameBoardState.Idle);
                    }
                    else
                    {
                        Block block = Block.CreateBlockObject(row, column, (int)BlockType.None, _controller._blockBoardObj.transform, _controller);
                        block.transform.localPosition = new Vector3(0, 0, -1);
                        _controller._blockMatrix[row, column] = block;
                        _controller._secondSelected = block;
                        _controller.ChangeToState(GameBoardState.Swap);
                    }
                }
                else
                {
                    if (selectedBlock.moveCnt != 0 || selectedBlock.fallCnt != 0 || selectedBlock.moveStay > 0)
                        return; // 不能和正在下落的方块交换

                    // check if the two selected block is adjacent
                    bool isAdjacent = _controller._firstSelected.CheckAdjacent(selectedBlock);
                    if (isAdjacent)
                    {
                        _controller._secondSelected = selectedBlock;
                        _controller.ChangeToState(GameBoardState.Swap);
                    }
                    else
                    {
                        if (selectedBlock != null && selectedBlock.Type != BlockType.None)
                        {
                            // Debug.Log(_controller._boardType + " -- selected block[" + row + "," + column + "]");
                            _controller._firstSelected.IsSelected = false;
                            _controller._firstSelected = selectedBlock;
                            _controller._firstSelected.IsSelected = true;
                            _controller.ChangeToState(GameBoardState.Selection);
                        }
                        else
                        {
                            _controller.ChangeToState(GameBoardState.Idle);
                        }
                    }
                }
            }
            else
            {
                _controller.ChangeToState(GameBoardState.Idle);
            }
        }
    }
}