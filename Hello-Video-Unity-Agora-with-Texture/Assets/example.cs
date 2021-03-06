﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;

// this is an example of using Agora unity sdk
// It demonstrates:
// How to enable video
// How to join/leave channel
// 
public class exampleApp : MonoBehaviour {

	public static void logD(string message){
		Debug.Log("zhangtest"+message);
	}

	// load agora engine
	public void loadEngine()
	{
		// start sdk
		logD ("initializeEngine");

		if (mRtcEngine != null) {
			logD ("Engine exists. Please unload it first!");
			return;
		}

		// init engine
		mRtcEngine = IRtcEngine.getEngine (mVendorKey);

		// enable log
		mRtcEngine.SetLogFilter (LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);
	}

	public void join(string channel)
	{
		logD ("calling join (channel = " + channel + ")");

		if (mRtcEngine == null)
			return;

		// set callbacks (optional)
		mRtcEngine.OnJoinChannelSuccess = onJoinChannelSuccess;
		mRtcEngine.OnUserJoined = onUserJoined;
		mRtcEngine.OnUserOffline = onUserOffline;
////		mRtcEngine.OnWarning += (int warn, string msg) => {
////		string descrition = IRtcEngineForGaming.GetErrorDescription(warn);
////	string message = "hehe";
////	Debug.Log(message);
////		string warningMessage = string.Format("onWarning callback {0} {1} {2}",warn,msg,descrition);
////		Debug.Log(warningMessage);
////
//		};
		//mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.GAME_FREE_MODE);
		// enable video
		mRtcEngine.EnableVideo();
		mRtcEngine.EnableVideoObserver();

		// allow camera output callback
		mRtcEngine.SetLogFilter(LOG_FILTER.DEBUG);

		// join channel
	   //mRtcEngine.EnableVideo();
		mRtcEngine.JoinChannel(channel, null, 0);
		
		logD ("initializeEngine done");
	}

	public void leave()
	{
		Debug.Log ("calling leave");

		if (mRtcEngine == null)
			return;

		// leave channel
		mRtcEngine.LeaveChannel();
		// deregister video frame observers in native-c code
		mRtcEngine.DisableVideoObserver();
	}

	// unload agora engine
	public void unloadEngine()
	{
		logD ("calling unloadEngine");

		// delete
		if (mRtcEngine != null) {
			IRtcEngine.Destroy ();
			mRtcEngine = null;
		}
	}

	// accessing GameObject in Scnene1
	// set video transform delegate for statically created GameObject
	public void onScene1Loaded()
	{
		GameObject go = GameObject.Find ("Cylinder");
		if (ReferenceEquals (go, null)) {
			logD ("BBBB: failed to find Cylinder");
			return;
		}
		VideoSurface o = go.GetComponent<VideoSurface> ();
		o.mAdjustTransfrom += onTransformDelegate;
	}

	// instance of agora engine
	public IRtcEngine mRtcEngine;
	private string mVendorKey = "f4637604af81440596a54254d53ade20";

	// implement engine callbacks

	public uint mRemotePeer = 0; // insignificant. only record one peer


	private void onWarning(int warningCode, string message){

	Debug.Log ("onWarning  code = "+warningCode +"  message = "+message);

	}

	private void onJoinChannelSuccess (string channelName, uint uid, int elapsed)
	{
		logD ("JoinChannelSuccessHandler: uid = " + uid);
	}

	// When a remote user joined, this delegate will be called. Typically
	// create a GameObject to render video on it
	private void onUserJoined(uint uid, int elapsed)
	{
		logD ("onUserJoined: uid = " + uid);
		// this is called in main thread

		// find a game object to render video stream from 'uid'
		GameObject go = GameObject.Find (uid.ToString ());
		if (!ReferenceEquals (go, null)) {
			return; // reuse
		}

		// create a GameObject and assigne to this new user
		go = GameObject.CreatePrimitive (PrimitiveType.Plane);
		if (!ReferenceEquals (go, null)) {
			go.name = uid.ToString ();

			// configure videoSurface
			VideoSurface o = go.AddComponent<VideoSurface> ();
			o.SetForUser (uid);
			o.mAdjustTransfrom += onTransformDelegate;
			o.SetEnable (true);
			o.transform.Rotate (-90.0f, 0.0f, 0.0f);
			float r = Random.Range (-5.0f, 5.0f);
			o.transform.position = new Vector3 (0f, r, 0f);
			o.transform.localScale = new Vector3 (0.5f, 0.5f, 1.0f);
		}
		mRemotePeer = uid;
	}

	// When remote user is offline, this delegate will be called. Typically
	// delete the GameObject for this user
	private void onUserOffline(uint uid, USER_OFFLINE_REASON reason)
	{
		// remove video stream
		logD ("onUserOffline: uid = " + uid);
		// this is called in main thread
		GameObject go = GameObject.Find (uid.ToString());
		if (!ReferenceEquals (go, null)) {
			Destroy (go);
		}
	}

	// delegate: adjust transfrom for game object 'objName' connected with user 'uid'
	// you could save information for 'uid' (e.g. which GameObject is attached)
	private void onTransformDelegate (uint uid, string objName, ref Transform transform)
	{
		if (uid == 0) {
			transform.position = new Vector3 (0f, 2f, 0f);
			transform.localScale = new Vector3 (2.0f, 2.0f, 1.0f);
			transform.Rotate (0f, 1f, 0f);
		} else {
			transform.Rotate (0.0f, 1.0f, 0.0f);
		}
	}
}
