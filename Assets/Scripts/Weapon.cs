using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

namespace Com.MultiPlayerProject
{
    public class Weapon : MonoBehaviourPunCallbacks
    {
        #region Variables
        public Gun[] loadOut;

        [HideInInspector] public Gun currentGunData;


        public Transform weaponParent;
        public GameObject bulletHolePrefab;
        public LayerMask canBeShot;
        public bool isAiming = false;
        public AudioSource sfx;
        public AudioClip hitmarkerSound;
        private float currentCooldown;
        private int currentIndex;
        private GameObject currentWeapon;
        private Image hitmarkerImage;
        private float hitmarkerFWait;

        private Color CLEARWHITE = new Color(1, 1, 1, 0);

        #endregion

        #region MonoBehaviorCallbacks

        private void Start()
        {
            hitmarkerImage = GameObject.Find("HUD/Hitmarker/Image").GetComponent<Image>();
            hitmarkerImage.color = CLEARWHITE;
        }

        private void Update()
        {

            if (Pause.paused && photonView.IsMine)
            {
                return;
            }


            if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1))
            {
                photonView.RPC("Equip", RpcTarget.All, 0);
            }
            if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha2))
            {
                photonView.RPC("Equip", RpcTarget.All, 1);
            }

            if (currentWeapon != null)
            {
                if (photonView.IsMine)
                {
                    Aim(Input.GetMouseButton(1));

                    if (!loadOut[currentIndex].automatic)
                    {
                        if (Input.GetMouseButtonDown(0) && currentCooldown <= 0f)
                        {
                            photonView.RPC("Shoot", RpcTarget.All);
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButton(0) && currentCooldown <= 0f)
                        {
                            photonView.RPC("Shoot", RpcTarget.All);
                        }
                    }

                    //cooldown
                    if (currentCooldown > 0)
                    {
                        currentCooldown -= Time.deltaTime;
                    }
                }


                // weapon position lock after recoil
                currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4);
            }

            if (photonView.IsMine)
            {
                if (hitmarkerFWait > 0)
                {
                    hitmarkerFWait -= Time.deltaTime;
                }
                else
                {
                    hitmarkerImage.color = Color.Lerp(hitmarkerImage.color, CLEARWHITE, Time.deltaTime * 2f);
                }
            }

        }
        #endregion

        #region Private Methods

        [PunRPC]
        void Equip(int p_ind)
        {
            if (currentWeapon != null)
            {
                Destroy(currentWeapon);
            }

            currentIndex = p_ind;

            GameObject t_newWeapon = Instantiate(loadOut[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
            t_newWeapon.transform.localPosition = Vector3.zero;
            t_newWeapon.transform.localEulerAngles = Vector3.zero;
            t_newWeapon.GetComponent<Sway>().isMine = photonView.IsMine;

            currentWeapon = t_newWeapon;
            currentGunData = loadOut[p_ind];
        }

        void Aim(bool p_isAiming)
        {
            isAiming = p_isAiming;
            Transform t_anchor = currentWeapon.transform.Find("Anchor");
            Transform t_states_ads = currentWeapon.transform.Find("States/ADS");
            Transform t_states_hip = currentWeapon.transform.Find("States/Hip");

            if (p_isAiming)
            {
                //aim
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_states_ads.position, Time.deltaTime * loadOut[currentIndex].aimSpeed);
            }
            else
            {
                //hip
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_states_hip.position, Time.deltaTime * loadOut[currentIndex].aimSpeed);

            }
        }

        [PunRPC]
        void Shoot()
        {
            
            Transform t_spawn = transform.Find("Cameras/Normal Camera");
            
            //setup bloom
            Vector3 t_bloom = t_spawn.position + t_spawn.forward * 1000f;
            t_bloom += Random.Range(-loadOut[currentIndex].bloom, loadOut[currentIndex].bloom) * t_spawn.up;
            t_bloom += Random.Range(-loadOut[currentIndex].bloom, loadOut[currentIndex].bloom) * t_spawn.right;
            t_bloom -= t_spawn.position;
            t_bloom.Normalize();
            
            //cooldown
            currentCooldown = loadOut[currentIndex].fireRate;
            
            //raycast
            RaycastHit t_hit = new RaycastHit();
            if(Physics.Raycast(t_spawn.position, t_bloom, out t_hit, 1000f, canBeShot))
            {
                GameObject t_newHole = Instantiate(bulletHolePrefab, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity) as GameObject;
                t_newHole.transform.LookAt(t_hit.point + t_hit.normal);
                Destroy(t_newHole, 5f);

                if (photonView.IsMine)
                {
                    //shooting other players on network
                    if (t_hit.collider.gameObject.layer == 10)
                    {
                        //give damage
                        t_hit.collider.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadOut[currentIndex].damage);

                        //show hitmarker
                        hitmarkerImage.color = Color.white;
                        sfx.PlayOneShot(hitmarkerSound);
                        hitmarkerFWait = 1f;
                    }
                }
            }

            //sound
            sfx.Stop();
            sfx.clip = currentGunData.gunshotSound;
            sfx.pitch = 1 - currentGunData.pitchRandomization + Random.Range(-currentGunData.pitchRandomization, currentGunData.pitchRandomization);
            sfx.Play();


            //gun fx
            currentWeapon.transform.Rotate(-loadOut[currentIndex].recoil, 0, 0);
            currentWeapon.transform.position -= currentWeapon.transform.forward * loadOut[currentIndex].kickback;

        }

        [PunRPC]
        private void TakeDamage(int p_damage)
        {
            GetComponent<Player>().TakeDamage(p_damage);
        }

        #endregion
    }
}
