using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box  {
	
	public Box(int fromX, int toX, int fromY, int toY){
		this.fromX = fromX;
		this.toX = toX;
		this.fromY = fromY;
		this.toY = toY;
	}
	public int fromX;
	public int fromY;
	public int toX;
	public int toY;

	public void grow(int[,] grid){
		if (Random.Range (0f, 1f) < 0.5f) {
			if (!GrowTopDown (grid)) {
				GrowLeftRight (grid);
			}
		} else {
			if (!GrowLeftRight (grid)) {
				GrowTopDown (grid);
			}
		}

	}
	private float prob = 0.25f;
	private bool GrowTopDown(int[,] grid){
		bool r= false;
		bool flag = true;
		for (int i = fromX-1; i <= toX+1; i++)
			if (grid [fromY -1 , i] != 0 || grid [fromY - 2,i] != 0)
				flag = false;

		if (flag && Random.Range(0f,1f) < prob) {
			r = true;
			for (int i = fromX; i <= toX; i++)
				grid [fromY-1,i] = -4;
			this.fromY--;
		}
		//========================================
		flag = true;
		for (int i = fromX-1; i <= toX+1; i++)
			if (grid [toY +1 , i] != 0 || grid [toY +2,i] != 0)
				flag = false;

		if (flag && Random.Range(0f,1f) < prob) {
			r = true;
			for (int i = fromX; i <= toX; i++)
				grid [toY+1,i] = -5;
			this.toY++;
		}

		return r;
	}

	private bool GrowLeftRight(int[,] grid){
		bool r = false;
		bool flag = true;
		for (int j = fromY - 1; j <= toY + 1; j++) {
			//Debug.Log (j + " " + fromX + " " + grid.GetLength(0) + " "  + grid.GetLength(1));
			if (grid [j, fromX - 1] != 0 || grid [j, fromX - 2] != 0)
				flag = false;
		}

		if (flag && Random.Range(0f,1f) < prob) {
			r = true;
			for (int j = fromY; j <= toY; j++)
				grid [j, fromX - 1] = -2;
			this.fromX--;
		}
		//========================================
		flag = true;
		for (int j = fromY-1; j <= toY+1; j++)
			if (grid [j, toX + 1] != 0 || grid [j, toX + 2] != 0)
				flag = false;

		if (flag && Random.Range(0f,1f) < prob) {
			r = true;
			for (int j = fromY; j <= toY; j++)
				grid [j, toX + 1] = -3;
			this.toX++;
		}
		return r;
	}


	public void VertcialColoring(int[,] grid){
		BlockGenerator.Counter++;
		int extra = Random.Range (1, 2);
		if (Random.Range(0, 4) == 0)
			extra *= 2;
		
		for (int j = fromY; j <= toY; j++) {
			for (int i = toX; i > toX-extra; i--)
				grid [j, i] = BlockGenerator.Counter;
		}


		extra = Random.Range(1,2);
		if (Random.Range(0, 4) == 0)
			extra *= 2;
		
		BlockGenerator.Counter++;
		for (int j = fromY; j <= toY; j++) {
			for (int i = fromX; i < fromX+extra; i++)
				grid [j, i] = BlockGenerator.Counter;
		}

	}

	public void HorizontalColoring(int[,] grid,HashSet<int> importantRectangles){
		BlockGenerator.Counter++;
		int extra = Random.Range (1, 2);
		//extra = 0;
		for (int i = fromX; i <= toX; i++) {
			for (int j = toY + 1; j > toY - extra; j--)
				grid [j, i] = BlockGenerator.Counter;
		}
		importantRectangles.Add(BlockGenerator.Counter);
	}
}
