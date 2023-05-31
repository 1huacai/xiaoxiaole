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
        _controller._firstSelected.IsSelected = true;
        _controller._firstSelected.ResetDragPos();
    }

    public override void OnBlockOperation(int row, int column, BlockOperation operation)
    {
        base.OnBlockOperation(row, column, operation);
        var first = _controller._firstSelected;
        if (first.fallCnt != 0 || first.moveCnt != 0)
        {
            _controller.ChangeToState(GameBoardState.Idle);
            return;
        }

        Debug.Log(_controller._boardType + " -- operation - Block[" + first.Row + "," + first.Column + " - " + first.Type + "] " + operation);
        if (operation == BlockOperation.TouchUp)
        {
            _controller.ChangeToState(GameBoardState.Idle);
            return;
        }
        if (operation == BlockOperation.DragHalf)
        {
            if (row >= 0 && row < _controller._curRowCnt && column >= 0 && column < Config.columns)
            {
                var selectedBlock = _controller._blockMatrix[row, column];
                Debug.Log(_controller._boardType + "--- second select block[" + row + "," + column + "] -- " + (selectedBlock == null ? "is null" : "is not null"));
                if (selectedBlock == null)
                {
                    // 不能和压力块交换
                    var pressure = _controller.GetPressureByRow(row);
                    if (pressure != null && pressure.xNum > column)
                        return;

                    // 只能和空方块进行水平交换
                    if (first.Row != row || System.Math.Abs(first.Column - column) != 1 || column < 0 || column >= Config.columns)
                    {
                        _controller.ChangeToState(GameBoardState.Idle);
                    }
                    else
                    {
                        // 不能和上方正在下落的空方块进行水平交换
                        Block above = _controller._blockMatrix[row + 1, column];
                        if (above != null && above.IsLocked == false)
                            return;

                        Block block = Block.CreateBlockObject(row, column, (int)BlockType.None, _controller._blockBoardObj.transform, _controller);
                        block.transform.localPosition = new Vector3(0, 0, -1);
                        _controller._blockMatrix[row, column] = block;
                        _controller._secondSelected = block;
                        _controller.ChangeToState(GameBoardState.Swap);
                    }
                }
                else
                {
                    if (selectedBlock.IsLocked // 锁住的方块
                    || selectedBlock.moveCnt != 0  // 正在移动的方块
                    || selectedBlock.fallCnt != 0 // 正在下落的方块
                    || selectedBlock.MoveStay > 0) // 可能下落的方块(帧动画计算延时的容错)
                        return; // 不能和正在下落的方块交换

                    // check if the two selected block is adjacent
                    bool isAdjacent = first.CheckAdjacent(selectedBlock);
                    if (isAdjacent)
                    {
                        _controller._secondSelected = selectedBlock;
                        _controller.ChangeToState(GameBoardState.Swap);
                    }
                    else
                    {
                        if (selectedBlock != null && selectedBlock.Type != BlockType.None)
                        {
                            first.IsSelected = false;
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