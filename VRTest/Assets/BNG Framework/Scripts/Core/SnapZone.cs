using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace BNG
{
    public class SnapZone : MonoBehaviour
    {

        /// <summary>
        /// If false, Item will Move back to inventory space if player drops it
        /// </summary>
        public bool CanDropItem = true;

        /// <summary>
        /// If false the swap zone cannot have it's content replaced.
        /// </summary>
        public bool CanSwapItem = true;

        /// <summary>
        /// Multiply Item Scale times this when in snap zone
        /// </summary>
        public float ScaleItem = 1f;
        private float _scaleTo;

        public bool DisableColliders = true;
        List<Collider> disabledColliders = new List<Collider>();

        /// <summary>
        /// Only snap if Grabbable was dropped maximum of X seconds ago
        /// </summary>
        public float MaxDropTime = 0.1f;

        /// <summary>
        /// If not empty, can only snap objects if transform name contains one of these strings
        /// </summary>
        public List<string> OnlyAllowNames;

        /// <summary>
        /// Do not allow snapping if transform contains one of these names
        /// </summary>
        public List<string> ExcludeTransformNames;

        public AudioClip SoundOnSnap;
        public AudioClip SoundOnUnsnap;

        /// <summary>
        /// Optional Unity Event  to be called when something is snapped to this SnapZone. Passes in the Grabbable that was attached.
        /// </summary>
        public GrabbableEvent OnSnapEvent;

        /// <summary>
        /// Optional Unity Event to be called when something has been detached from this SnapZone. Passes in the Grabbable is being detattached.
        /// </summary>
        public GrabbableEvent OnDetachEvent;

        GrabbablesInTrigger gZone;

        public Grabbable HeldItem;
        Grabbable trackedItem; // If we can't drop the item, track it separately

        // Closest Grabbable in our trigger
        public Grabbable ClosestGrabbable;

        SnapZoneOffset offset;

        // Start is called before the first frame update
        void Start()
        {
            gZone = GetComponent<GrabbablesInTrigger>();
            _scaleTo = ScaleItem;

            // Auto Equip item
            if (HeldItem != null)
            {
                Snap(HeldItem);
            }
        }


        // Update is called once per frame
        void Update()
        {

            ClosestGrabbable = getClosestGrabbable();

            // Can we grab something
            if (HeldItem == null && ClosestGrabbable != null)
            {
                Debug.Log("here");
                float secondsSinceDrop = Time.time - ClosestGrabbable.LastDropTime;
                if (secondsSinceDrop < MaxDropTime)
                {
                    Debug.Log("B");
                    Snap(ClosestGrabbable);
                }
                else if (movingGrabbable == null)
                {
                    Debug.Log("Snap");
                    Snap(ClosestGrabbable);
                }
            }

            // Keep snapped to us or drop
            if (HeldItem != null)
            {
                // Something picked this up or changed transform parent
                if (HeldItem.BeingHeld || HeldItem.transform.parent != transform)
                {
                    //   ReleaseAll();
                }
                else
                {
                    // Scale Item while inside zone.                                            
                    float localScale = HeldItem.OriginalScale * _scaleTo;
                    HeldItem.transform.localScale = Vector3.Lerp(HeldItem.transform.localScale, new Vector3(localScale, localScale, localScale), Time.deltaTime * 30f);

                    // Make sure this can't be grabbed from the snap zone
                    if (HeldItem.enabled || (disabledColliders.Count > 0 && disabledColliders[0] != null && disabledColliders[0].enabled))
                    {
                        disableGrabbable(HeldItem);
                    }

                    // Lock into place
                    if (offset)
                    {
                        //  HeldItem.transform.localPosition = offset.LocalPositionOffset;
                        //   HeldItem.transform.localEulerAngles = offset.LocalRotationOffset;
                    }
                    else
                    {
                        //  HeldItem.transform.localPosition = Vector3.zero;
                        //   HeldItem.transform.localEulerAngles = Vector3.zero;
                    }

                }
            }

            // Can't drop item. Lerp to position if not being held
            if (!CanDropItem && trackedItem != null && HeldItem == null)
            {
                if (!trackedItem.BeingHeld)
                {
                    Snap(trackedItem);
                }
            }
        }

        Grabbable getClosestGrabbable()
        {
            Grabbable closest = null;
            float lastDistance = 9999f;

            if (gZone == null || gZone.NearbyGrabbables == null)
            {
                return null;
            }

            foreach (var g in gZone.NearbyGrabbables)
            {

                // Collider may have been disabled
                if (g.Key == null)
                {
                    continue;
                }

                float dist = Vector3.Distance(transform.position, g.Value.transform.position);

                if (dist < lastDistance)
                {

                    //  Not allowing secondary grabbables such as slides
                    if (g.Value.OtherGrabbableMustBeGrabbed != null)
                    {
                        continue;
                    }

                    // Don't allow SnapZones in SnapZones
                    if (g.Value.GetComponent<SnapZone>() != null)
                    {
                        continue;
                    }

                    // Don't allow InvalidSnapObjects to snap
                    if (g.Value.CanBeSnappedToSnapZone == false)
                    {
                        continue;
                    }



                    // Only valid to snap if being held or recently dropped
                    if (g.Value.BeingHeld || (Time.time - g.Value.LastDropTime < MaxDropTime))
                    {
                        closest = g.Value;
                        lastDistance = dist;
                    }
                }
            }

            return closest;
        }

        public void Snap(Grabbable grab)
        {

            // Grab is already in Snap Zone
            if (grab.transform.parent != null && grab.transform.parent.GetComponent<SnapZone>() != null)
            {
                return;
            }

            if (HeldItem != null)
            {
                ReleaseAll();
            }

            HeldItem = grab;

            // Set scale factor            
            // Use SnapZoneScale if specified
            if (grab.GetComponent<SnapZoneScale>())
            {
                _scaleTo = grab.GetComponent<SnapZoneScale>().Scale;
            }
            else
            {
                _scaleTo = ScaleItem;
            }

            // Is there an offset to apply?
            //SnapZoneOffset off = grab.GetComponent<SnapZoneOffset>();
            //if (off)
            //{
            //    offset = off;
            //}
            //else
            //{
            //    offset = grab.gameObject.AddComponent<SnapZoneOffset>();
            //    offset.LocalPositionOffset = Vector3.zero;
            //    offset.LocalRotationOffset = Vector3.zero;
            //}

            // Disable the grabbable. This is picked up through a Grab Action
            disableGrabbable(grab);

            Move(grab);


            // Call event
            if (OnSnapEvent != null)
            {
                OnSnapEvent.Invoke(grab);
            }
        }

        [SerializeField] Transform aboveCap;
        private Grabbable movingGrabbable;

        private void Move(Grabbable grab)
        {
            movingGrabbable = grab;

            grab.transform.SetParent(transform, true);
            iTween.MoveTo(grab.gameObject, iTween.Hash("position", aboveCap.localPosition, "time", 0.4f, "islocal", true, "oncomplete", "StartMovingFinal", "oncompletetarget", gameObject, "easetype", iTween.EaseType.easeInOutCirc));
            iTween.RotateTo(grab.gameObject, iTween.Hash("rotation", new Vector3(0, 0, 90), "time", 0.4f, "easetype", iTween.EaseType.linear, "islocal", true));
            grab.GetComponentInChildren<Rigidbody>().isKinematic = true;

            grab.GetComponent<Collider>().enabled = false;
            grab.GetComponent<Collider>().isTrigger = true;
        }

        void StartMovingFinal()
        {
            iTween.MoveTo(movingGrabbable.gameObject, iTween.Hash("position", Vector3.zero, "time", 0.6f, "islocal", true));
            iTween.RotateTo(movingGrabbable.gameObject, iTween.Hash("rotation", new Vector3(180, 0, 90), "time", 0.6f, "easetype", iTween.EaseType.linear, "islocal", true));
        }

        void MoveComplete()
        {
            //Debug.Log("Complete");
            //movingGrabbable.transform.SetParent(transform);
        }


        void disableGrabbable(Grabbable grab)
        {

            if (DisableColliders)
            {
                disabledColliders = grab.GetComponentsInChildren<Collider>(false).ToList();
                for (int x = 0; x < disabledColliders.Count; x++)
                {
                    Collider c = disabledColliders[x];
                    c.enabled = false;
                }
            }

            // Disable the grabbable. This is picked up through a Grab Action
            grab.enabled = false;
        }

        public void GrabEquipped(Grabber grabber)
        {

            if (grabber != null)
            {
                if (HeldItem)
                {
                    var g = HeldItem;
                    ReleaseAll();

                    // Position next to grabber if somewhat faraways
                    if (Vector3.Distance(g.transform.position, grabber.transform.position) > 0.2f)
                    {
                        // g.transform.position = grabber.transform.position;
                    }

                    // Do grab
                    grabber.GrabGrabbable(g);
                }
            }
        }

        /// <summary>
        /// Release  everything snapped to us
        /// </summary>
        public void ReleaseAll()
        {

            // No need to keep checking
            if (HeldItem == null)
            {
                return;
            }

            // Still need to keep track of item if we can't fully drop it
            if (!CanDropItem && HeldItem != null)
            {
                trackedItem = HeldItem;
            }

            HeldItem.ResetScale();

            if (DisableColliders && disabledColliders != null)
            {
                foreach (var c in disabledColliders)
                {
                    if (c)
                    {
                        c.enabled = true;
                    }
                }
            }
            disabledColliders = null;

            HeldItem.enabled = true;
            HeldItem.transform.parent = null;

            // Play Unsnap sound
            if (HeldItem != null)
            {
                // Call event
                if (OnDetachEvent != null)
                {
                    OnDetachEvent.Invoke(HeldItem);
                }
            }

            HeldItem = null;
        }
    }
}
