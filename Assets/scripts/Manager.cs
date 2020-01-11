using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour {

	[SerializeField]
	private BlockGenerator BlockGeneratorPrefab;

	private float unitSize = 0.125f;
	private BlockGenerator _currentBlock;
	void Start () {
//		Random.InitState(3006);
		Physics2D.autoSimulation = false;
		if(tests < 0)
			GenerateRandomBlock();
	}

	private void GenerateRandomBlock()
	{
		var width = Random.Range (5, 10) * 5;
		var height = Random.Range (5, 10) * 5;
		//var minEmpty = Random.Range (8, 10);
		//var maxEmpty = Random.Range (8, 10);
		var minEmpty = 8;
		var maxEmpty = 12;
		var g = Instantiate (BlockGeneratorPrefab.gameObject,gameObject.transform);
		g.transform.localPosition = new Vector2 (0, 0);

		
		_currentBlock = g.GetComponent<BlockGenerator>();
		_currentBlock.Init (width, height, minEmpty, maxEmpty);
		
		transform.localScale = new Vector3(1.5f,1.5f,1.4f);
	}
	
	private int c = 0;
	private int stables = 0;
	private int tests = -200;
	void Update () {
		
		if (c < tests)
		{
			GenerateRandomBlock();
			
			if (_currentBlock.IsStable())
			{
				stables++;
				_currentBlock.Analyze();
			}
			
			DestroyImmediate(_currentBlock.gameObject);
			Debug.Log(c);
		}else if (c == tests)
		{
			Debug.Log(stables +"/" + tests);
		}


		if (Input.GetKeyDown (KeyCode.R)) {
			UnityEngine.SceneManagement.SceneManager.LoadScene ("s2");
		}

		if (Input.GetKeyDown(KeyCode.S))
		{
			Physics2D.autoSimulation = true;
		}

		if (Input.GetKeyDown(KeyCode.C))
		{
			Debug.Log(_currentBlock.IsStable());
		}
		c++;
		
	}

}
