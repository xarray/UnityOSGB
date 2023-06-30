using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    // Start is called before the first frame update
    void Start()
    {
        Uri ur = new Uri("https://ssa.com/sdasd/sdas/asd.text");
        
        print(ur.AbsolutePath);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
