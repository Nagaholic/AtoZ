using UnityEngine;
using System.Collections;

public class TypeAlphabets : MonoBehaviour {

	public GameObject _gameobj_atoz;
	UnityEngine.UI.InputField _text_atoz;
	public GameObject _gameobj_time;
	UnityEngine.UI.Text _text_time;
	public GameObject _gameobj_name;
	UnityEngine.UI.InputField _text_name;
	bool _started=false;
	bool _finished=false;
	float _time=0.0f;
	// Use this for initialization
	IEnumerator Start () {

		//	setup.
		_text_atoz=_gameobj_atoz.GetComponent<UnityEngine.UI.InputField>();
		_text_time=_gameobj_time.GetComponent<UnityEngine.UI.Text>();
		_text_name=_gameobj_name.GetComponent<UnityEngine.UI.InputField>();
		StartCoroutine(Get());

		yield return null;

		//	game main.
		while (!_finished)
		{
			if (_text_atoz.text.Length > 0)
			{
				if (!_started) 
				{
					_started=true;
				}
			}
			if (_started && !_finished)
			{
				if (_time < 60.0f)
				{
					_time += Time.deltaTime;
				}
			}
			_text_time.text = _time.ToString();
			if (_text_atoz.text == "abcdefghijklmnopqrstuvwxyz")
			{
				_finished=true;
				//post time.
			}
			yield return null;
		}

		//	finished.
		yield return StartCoroutine(Post());
	}
	
	
	IEnumerator Get()
	{
		float wait = 0.0f;
		while (true)
		{
			wait -= Time.deltaTime;
			if (wait < 0.0f)
			{
				Debug.Log("Start Get");
				wait = 2.0f;

				WWW w = new WWW("localhost:10080");
				yield return w;

				if (!string.IsNullOrEmpty(w.error))
				{
					Debug.LogError("server error="+w.error);
				}
				
				//データフォーマット
				//	time:float 4byte
				//	name:char[] _text_atoz.characterLimit(16byte)
				//+	-----------------------------------------------
				//	20byte
				int data_size=20;
				float[] times = new float[3];
				string[] names = new string[3];
				string[] gameobj_names=new string[] {"rank1st", "rank2nd", "rank3rd"};
				for (int i=0;i<3;++i)
				{
					times[i] = System.BitConverter.ToSingle(w.bytes, i*data_size);
					names[i] = System.Text.Encoding.UTF8.GetString(w.bytes, i*data_size+sizeof(float), _text_name.characterLimit);
					GameObject rank = GameObject.Find(gameobj_names[i]);
					if (rank)
					{
						rank.transform.FindChild("name").GetComponent<UnityEngine.UI.Text>().text = names[i];
						rank.transform.FindChild("time").GetComponent<UnityEngine.UI.Text>().text = times[i].ToString("F4");
					}
				}
				Debug.Log("End Get");
				
			}
			yield return null;
		}
	}
	IEnumerator Post()
	{
		Debug.Log("Start Post");

		WWWForm form = new WWWForm();
		form.AddBinaryData("time", System.BitConverter.GetBytes(_time));
		form.AddField("name", _text_name.name.Length > 0 ? _text_name.text : "NoName");

		WWW w = new WWW("localhost:10080", form);
		yield return w;

		if (!string.IsNullOrEmpty(w.error))
		{
			Debug.LogError("server error="+w.error);
		}
		Debug.Log("End Get");
	}
}
