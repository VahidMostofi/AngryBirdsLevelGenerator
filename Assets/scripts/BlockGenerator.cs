using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlockGenerator : MonoBehaviour
{
	private readonly Vector2[] _predefinedRectangles = new Vector2[]
	{
		new Vector2(4,4),
		new Vector2(6,1),
		new Vector2(1,6),
		
		new Vector2(4,2),
		new Vector2(2,4),

		new Vector2(2,2),		
		new Vector2(1,2),
		new Vector2(2,1),
		new Vector2(1,1),
	};
	public static int Counter = 0;
	public GameObject Unit;

	[SerializeField]
	private GameObject _debugDrawTile; //OnlyForDebugDraw

	private Transform _tiles;
	private Transform _blocks;

	private int _width ;
	private int _height;
	private int[,] _grid;

	private int _minSpaceSize;
	private int _maxSpaceSize;

	private readonly int sizeToCheckAroundBox = 1;
	private List<Rectangle> _rectangles;
	private HashSet<int> _importantRectangles; 
	private readonly List<Box> _boxes = new List<Box>();

	private int _stackMaxHeight;

	private Analysis _analysis;
	private long _id;
	

	public void Init(int width, int height,int minSpaceSize,int maxSpaceSize){
		
		_id = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		
		BlockGenerator.Counter = 0;
		_width = width;
		_height = height;
		_minSpaceSize = minSpaceSize;
		_maxSpaceSize = maxSpaceSize;
		
		_importantRectangles = new HashSet<int>();
		_rectangles = new List<Rectangle> ();
		_grid = new int[height,width];
		
		
		for (int i = 0; i < width * height * 10; i++)
		{
			RemoveRandomBox();
		}
		
		foreach (var b in _boxes) 
		{
			b.grow (_grid);
		}
		

		foreach (var b in _boxes) 
		{
			b.VertcialColoring (_grid);
		}

		foreach (var b in _boxes) 
		{
			b.HorizontalColoring (_grid,_importantRectangles);
		}
		
		RemoveBottomRows();

		FindRectsToHere ();

		RemoveTopMargins ();

		FindPredefinedRectangles();

		RemoveUnnecessaryBlocks ();

//		Draw ();

		AddPhysics ();
	}

	private void RemoveBottomRows()
	{
		var unnecessaryRows = 0;
		var flag = true;
		for (var j = 0; j < _height; j++)
		{
			for(var i = 0;i < _width;i++)
				if (_grid[j, i] != 0)
				{
					flag = false;
					break;
				}

			if (!flag)
			{
				unnecessaryRows = j + 1;
				break;
			}
		}

		var s = unnecessaryRows;
		
		int[,] temp = new int[_height - s, _width];
		for(var i = 0;i < _width;i++)
		for (var j = 0; j < _height - s; j++)
			temp[j, i] = _grid[j+s, i];
		_grid = temp;
		_height -= s;
		
				

		foreach (var box in _boxes)
		{
			box.fromY += unnecessaryRows;
			box.toY += unnecessaryRows;
		}

	}

	private void AddPhysics(){
		var all = "";

		if (_blocks != null) {
			Destroy (_blocks.gameObject);
		}
		_blocks = new GameObject ().transform;
		_blocks.gameObject.name = "blocks";
		_blocks.SetParent (gameObject.transform);
		_blocks.localPosition = Vector2.zero;

		foreach (var rect in this._rectangles) {
			all += (rect.toX - rect.fromX + 1) + " by " + (rect.toY - rect.fromY + 1) + "\n";

			var r = Random.Range(0f,1f);
			var g = Instantiate (Unit,_blocks.transform);
			var c = 1.0f;
			var w = rect.toX - rect.fromX + 1;
			var h = rect.toY - rect.fromY + 1;
			
			var spriteName = "None";
//			if (w == 4 && h == 4)
//			{
//				spriteName = "4a4a";
//			}
//			else if (w == 2 && h == 4)
//			{
//				spriteName = "2a4a";
//			}
//			else if (w == 4 && h == 2)
//			{
//				spriteName = "4a2a";
//			}
//			else if (w == 1 && h == 6)
//			{
//				spriteName = "1a6a";
//			}
//			else if (w == 6 && h == 1)
//			{
//				spriteName = "6a1a";
//			}
//			else if (w == 2 && h == 2)
//			{
//				spriteName = "2a2a";
//			}
//			else
//			{
//				Debug.Log(w + " " + h);
//			}
//			
			if (!spriteName.Equals("None"))
			{
				g.transform.localPosition = ConvertPosition((rect.toX + rect.fromX) / 2f, (rect.toY + rect.fromY) / 2f);
				g.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(spriteName);
				g.GetComponent<Rigidbody2D> ().mass = w * h * 1f;
				g.GetComponent<BoxCollider2D>().size = new Vector2(g.GetComponent<BoxCollider2D>().size.x * w, g.GetComponent<BoxCollider2D>().size.y * h);
			}
			else
			{
				g.transform.localScale = new Vector2 ((rect.toX - rect.fromX+1) * c, (rect.toY - rect.fromY+1) * c);
				g.transform.localPosition = ConvertPosition((rect.toX + rect.fromX) / 2f, (rect.toY + rect.fromY) / 2f);
				g.GetComponent<Rigidbody2D> ().mass = g.transform.localScale.x * g.transform.localScale.y * 1f;
				g.GetComponent<SpriteRenderer> ().color = new Color (0, r, r,1f);
			}
		}

		Debug.Log("We have physics");
	}

	private Vector2 ConvertPosition(float i,float j){
		return new Vector2 ((i - 0.873f*i), (j - 0.873f*j)); //todo make this better
	}

	private void FindRectsToHere(){//it would be nice if it could find them if they are connectd to each other

		
		for (var c = 1; c <= BlockGenerator.Counter; c++) {
			var fromX = 1000;
			var fromY = 1000;
			var toX = -1;
			var toY = -1;
			for (var i = 0; i < _width; i++) {
				for (var j = 0; j < _height; j++) {

					if (_grid [j, i] == c) {

						if (i < fromX) {
							fromX = i;
						}
						if (i > toX) {
							toX = i;
						}
						if (j < fromY) {
							fromY = j;
						}
						if (j > toY) {
							toY = j;
						}

					}

				}
			}
			if (fromX != 1000 && toX != -1) {
				_rectangles.Add (new Rectangle (fromX, toX, fromY, toY));

			}
			fromX = fromY = 1000;
			toX = toY = -1;
		}
	}


	private void Draw(){

		if (_tiles != null) {
			Destroy (_tiles.gameObject);
		}
		_tiles = new GameObject ().transform;
		_tiles.gameObject.name = "tiles";
		_tiles.SetParent (gameObject.transform);
		_tiles.localPosition = Vector2.zero;

		var alpha = 1.0f;
		for (var i = 0; i < _width; i++) {
			for (var j = 0; j < _height; j++) {
//				if (_grid [j, i] < 0)
//					continue;
				var g = Instantiate (_debugDrawTile);
				g.transform.SetParent (_tiles);
				g.transform.localScale = new Vector2 (0.25f, 0.25f);
				if (_grid [j, i] == -2)
					g.GetComponent<SpriteRenderer> ().color = Color.red;
				if (_grid [j, i] == -3)
					g.GetComponent<SpriteRenderer> ().color = Color.blue;
				if (_grid [j, i] == -4)
					g.GetComponent<SpriteRenderer> ().color = Color.green;
				if (_grid [j, i] == -5)
					g.GetComponent<SpriteRenderer> ().color = Color.black;
				
				g.transform.GetChild (0).GetComponent<TextMesh> ().text = "" + _grid [j, i];
				g.transform.localPosition = ConvertPosition(i,j); //todo not literals
			}
		}
	}

	private void RemoveTopMargins(){
		for (var i = 0; i < _width; i++) {
			var j = _height - 1;

			while (j >= 0 && _grid [j, i] == 0) {
				_grid [j, i] = -10;
				j--;
			}
		}
	}

	private void RemoveLeftRightMargins(){
		for (var j = 0; j < _height; j++) {
			var i = 0;
			while (i < _width && _grid [j, i] == 0) {
				_grid [j, i] = -11;
				i++;
			}
			i = _width - 1;
			while (i >=0 && _grid [j, i] == 0) {
				_grid [j, i] = -12;
				i--;
			}
		}
	}

	private void RemoveZeros(){
		for (int j = 0; j < _height; j++) {
			for (int i = 0; i < _width; i++) {
				if (_grid [j, i] == 0) {
					_grid [j, i] = -15;
				}
			}
		}
	}

	private void FindPredefinedRectangles()
	{
		for (var j = 0; j < _height; j++)
		{
			for (var i = 0; i < _width; i++)
			{
				foreach (var definedRect in _predefinedRectangles)
				{
					ColorIfCan(i, j, (int) definedRect.x, (int) definedRect.y);
					Debug.Log("X "+ (int) definedRect.x + " " +  (int) definedRect.y);
					ColorIfCan(i, j, (int) definedRect.y, (int) definedRect.x);
				}
			}
		}
		
		for (var j = 0; j < _height; j++)
		{
			for (var i = 0; i < _width; i++)
			{
				foreach (var definedRect in _predefinedRectangles)
				{
					if(definedRect.x - 1.0f < 1e-5 && definedRect.y - 8.0f < 1e-8)
						continue;
				//	ColorIfCan(i, j, (int) definedRect.y, (int) definedRect.x);
				}
			}
		}
	}

	private void FindRectangles(){
		
		for (var j = 0; j < _height; j++) {
			for (var i = 0; i < _width; i++) {
				if (_grid [j, i] == 0) {
					for(var w = 16; w >0;w--)
					{
						for (var h = 16; h >0; h--)
						{
							ColorIfCan(i, j, w, h);
						}
					}
				}
			}
		}
	}

	private bool ColorIfCan(int startX,int startY , int w, int h){
		if (HasRectangle (startX, startY, w, h)) {
			ColorRectangle (startX, startY, w, h);
			return true;
		}
		return false;
	}

	private bool HasRectangle(int startX,int startY, int w,int h){
		for (int i = startX; i < startX + w; i++) {
			for (int j = startY; j < startY + h; j++) {
				if (i >= _width)
					return false;
				if (j >= _height)
					return false;
				if (_grid [j, i] != 0  && !(i > startX && _grid[j,i] == -10))
					return false;
			}
		}
		return true;
	}

	private void ColorRectangle(int startX,int startY, int w,int h){
		BlockGenerator.Counter++;
		for (int i = startX; i < startX + w; i++) {
			for (int j = startY; j < startY + h; j++) {
				_grid [j, i] = BlockGenerator.Counter;
			}
		}

		_rectangles.Add(new Rectangle(startX,startX + w - 1,startY,startY+h-1));
	}

	private int CountEmpties(){
		int c = 0;
		for (int i = 0; i < _width; i++) {
			for (int j = 0; j < _height; j++) {
				if (_grid [j, i] != 0) {
					c++;
				}
			}
		}
		return c;
	}

	private bool RemoveRandomBox(){
		
		//choose a random width and a random height
		var w = Random.Range (this._minSpaceSize, this._maxSpaceSize);
//		Debug.Log(w);
		var h = Random.Range (this._minSpaceSize + 1, this._maxSpaceSize + 1);

		//try to find a nice place for the this width and height
		for (var i = 0; i < 100; i++) {
			var x = Random.Range (sizeToCheckAroundBox + 1, _width - w - sizeToCheckAroundBox + 1 - 1);
			var y = Random.Range (sizeToCheckAroundBox + 1, _height - h - sizeToCheckAroundBox + 1 - 1);

			//all the values in the box must be zero
			bool flag = true;
			for (var ii = x; ii < x + w; ii++) { //check
				for (var jj = y; jj < y + h; jj++) {
					if (_grid [jj, ii] < 0) { //places with less than zero are already removed out
						flag = false;
						ii = x + w + 1;
						break;
					}
				}
			}

			if (flag) {
				_boxes.Add(
					new Box(
						x+this.sizeToCheckAroundBox,
						x+w-this.sizeToCheckAroundBox-1,
						y+this.sizeToCheckAroundBox + 1,
						y+h-this.sizeToCheckAroundBox-1)
				);
				for (var ii = x+this.sizeToCheckAroundBox; ii < x + w-this.sizeToCheckAroundBox; ii++) { //check
					for (var jj = y+sizeToCheckAroundBox + 1; jj < y + h-sizeToCheckAroundBox; jj++) {
						_grid [jj, ii] = -1;
					}
				}

				return true;
			}
		}

		return false;
	}


	private void RemoveUnnecessaryBlocks(){

		var flag = true;
		while (flag)
		{
			flag = false;
			var idx = 0;
			foreach (var rect in this._rectangles)
			{
				flag = true;
				for (var i = rect.fromX; i <= rect.toX; i++)
				{
					if (_grid[rect.toY + 1, i] > 0 || _importantRectangles.Contains(_grid[rect.toY, i]))
					{
						flag = false;
						break;
					}
				}
				if (flag)
					break;

				idx++;
			}

			if (flag)
			{
				for (var i = _rectangles[idx].fromX; i <= _rectangles[idx].toX; i++)
				{
					for (var j = _rectangles[idx].fromY; j <= _rectangles[idx].toY; j++){
						_grid[j, i] = -1;
					}
				}

				_rectangles.RemoveAt(idx);
			}
		}
	}

	private int getStackHeight()
	{
		return _stackMaxHeight;
	}

	public bool IsStable()
	{
		float t2 = Time.realtimeSinceStartup;
		Physics2D.autoSimulation = false;
		List<float> velSums = new List<float>();
		float max = 0;
		var res = "";
		for (var i = 0; i < 60 * 60; i++)
		{
			Physics2D.Simulate(Time.fixedDeltaTime);
			if (i % 30 == 0)
			{
				float velSum = 0;
				foreach (Transform t in _blocks)
				{
					velSum += t.GetComponent<Rigidbody2D>().velocity.sqrMagnitude;
				}
				velSums.Add(velSum);
				if (velSum > max)
					max = velSum;
				res += velSum + " ";
			}
		}
		Physics2D.autoSimulation = true;
		return max < 10.0f;
	}

	public void Analyze()
	{
		_analysis = new Analysis
		{
			id = _id,
			Heights = new int[_width],
			Widths =  new int[_height],
			TargetWidth = _width,
			TargetHeight =  _height
		};
		
		for (var i = 0; i < _width; i++)
		{
			for (var j = _height-1; j >= 0; j--)
			{
				if (_grid[j, i] > 0)
				{
					if(j > _analysis.MaxHeight)
						_analysis.MaxHeight = j;
					else if (j < _analysis.MinHeight)
						_analysis.MinHeight = j;
					
					_analysis.Heights[i] = j;
					break;
				}
			}
		}

		for (var j = 0; j < _height; j++)
		{
			var w = _width;
			var i = 0;
			while (_grid[j, i] < 0 && w <_width && w >= 0)
				w++;
			i = _width - 1;
			while (_grid[j, i] < 0 && w <_width && w >= 0)
				w--;

			if (w < _analysis.MinWidth)
				_analysis.MinWidth = w;
			if (w > _analysis.MaxWidth)
				_analysis.MaxWidth = w;
			_analysis.Widths[j] = w;
		}
		
		_analysis.CreateJson();

	}
	
	void Update(){
//		if (Input.GetKeyDown (KeyCode.A)) {
//			RemoveUnnecessaryBlocks ();
//			AddPhysics();
//		}
	}
}
