using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ThrowBall : MonoBehaviour
{
    private List<Vector3> trackingPos = new List<Vector3>();
    public float velocity = 1000f;

    private bool pickedUp = false;
    private GameObject parentHand;
    private Rigidbody rb;

    private XRGrabInteractable grabInteractable; // Komponent XRGrabInteractable
    private XRController controller; // Referencja do kontrolera XR

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Pobieramy Rigidbody
        grabInteractable = GetComponent<XRGrabInteractable>(); // Pobieramy komponent XRGrabInteractable

        if (grabInteractable != null)
        {
            grabInteractable.onSelectEntered.AddListener(OnPickedUp);  // Nas�uchujemy zdarzenia wej�cia w interakcj�
            grabInteractable.onSelectExited.AddListener(OnDropped);    // Nas�uchujemy zdarzenia zako�czenia interakcji
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (pickedUp)
        {
            rb.useGravity = false;
            transform.position = parentHand.transform.position;
            transform.rotation = parentHand.transform.rotation;

            // �ledzenie pozycji, aby obliczy� kierunek rzutu
            if (trackingPos.Count > 15)
            {
                trackingPos.RemoveAt(0);
            }
            trackingPos.Add(transform.position);

            // Sprawdzamy, czy trigger zosta� zwolniony
            if (controller.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
            {
                if (triggerValue < 0.1f) // Kiedy trigger jest zwolniony
                {
                    Vector3 direction = trackingPos[trackingPos.Count - 1] - trackingPos[0];
                    rb.AddForce(direction * velocity);
                    rb.useGravity = true;
                    rb.isKinematic = false;
                    GetComponent<Collider>().isTrigger = false;
                    pickedUp = false;
                }
            }
        }
    }

    // Funkcja wywo�ywana, gdy obiekt jest podnoszony
    private void OnPickedUp(XRBaseInteractor interactor)
    {
        pickedUp = true;
        parentHand = interactor.gameObject; // Przypisujemy r�k�, kt�ra podnosi obiekt
        controller = interactor.GetComponent<XRController>(); // Przypisujemy kontroler
    }

    // Funkcja wywo�ywana, gdy obiekt jest upuszczany
    private void OnDropped(XRBaseInteractor interactor)
    {
        // Przy upuszczaniu obiektu, wy��czamy kinematyk� i w��czamy grawitacj�
        Vector3 direction = trackingPos[trackingPos.Count - 1] - trackingPos[0];
        rb.AddForce(direction * velocity);
        rb.useGravity = true;
        rb.isKinematic = false;
        GetComponent<Collider>().isTrigger = false;

        pickedUp = false; // Ustawiamy flag� pickedUp na false, gdy obiekt jest upuszczony
    }
}