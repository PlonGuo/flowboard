
import { Injectable, signal } from '@angular/core';
import { Board, Task } from '../models/board.model';

@Injectable({ providedIn: 'root' })
export class BoardService {
  private readonly boards = signal<Board[]>([
    {
      id: 'q4-marketing-sprint',
      title: 'Q4 Marketing Sprint',
      lastModified: '2h ago',
      progress: 75,
      imageUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuD4lJXBhbiBr09RWCWPiyZd00fDz1cKCFI4_5TFPELaFxM2eXf0f6e8FwZpjBcGK7JtNs5-xByXxr2STVpQ-jjZWlrflQQ_I4goeoJhK1upJyLViIEjZoS05KDVUeDR8rLM7RgRXJBA5B1JMWWzjLQZCj_3QSTzb2MTJLjJW5wpBlMHNWw1sRgXSyW9fEgLNseZebsqb7hTiXyK7iGamwbgd3WoqaobgvCDkIOb4_dtVB0XqNuigF5y8btYUiA4h5aCxdJpufHE-MlV',
      members: [
        { name: 'User 1', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuCaP_uk-OMMC5vTjXfzcGcXwjSYhdRcs-S4GfrsqUkDebXEbjnT-5AlWC6xmMCFQAPgGZ6zmHnVYaqgsiDg64QlBCM3VmgmdFYnhzz3AgYgQS0zP5IYU_4VgRbFA-etqzWhbJADUsCLwSEo621fjNE_mVjjBRkw2XMFCVffCX3rzeR73OMxheCJQnsWtktB5Xj_f4Z5cp25TcRMyeNpRpSH4Hale1yHv58JNfcBGhJFpDAsNF1va3G7S89uWbH2nW74kCAeP-ceW_sv' },
        { name: 'User 2', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuCXVsEZJs6g-b-grLcCaAsl3KPPG8boekPnpDnSf7KCHoALI9Bcn8xgX_roLs53TOKPaENs3hLESBfE6yxi83sokbRtmmA_59uSMRPB4b36gkyNt6SY32U7_4YyL3dZcFUKs6ulwgFMAX7ZndMRrV8Yc3C62K479JmR524G6h7mOaayav4gpDRA1B9nyUgGOb71T6X48NZOC8Zu7L2d31OJ5_hDAxXuZRjM40L5V-GG7fEuOduN9YgHhLt_bBtzOIr_O6ForG_jYi-J' },
        { name: 'User 3', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuDSaPaHjBndaoNKCgEYlfB8LApfuXbyA1zwUY0a_Mw_1Q6glw3mqVdtFI4SUAdwcQ8uxwjw7kTFhT3X-BsOKuu_FVTnK1PpfyC_FunG9Hk40BAofSinIaLhfFSOlIbVQtSCYPBZ8LhzRl5SFR8FUacFrMjz-JltYI9pkm-864837rwtWob6KO-zygE4-zUglEgCwbr03Q8FF-dCtdsXApdzbKdcmMAdLWYxh7bW9QodCEjq7mZ2dNx27SQ_8ZTsYOmM1CGjt7m4Q1Lk' }
      ],
      moreMembersCount: 5,
      columns: [],
      tasks: []
    },
    {
      id: 'product-roadmap-2024',
      title: 'Product Roadmap 2024',
      lastModified: '5h ago',
      progress: 40,
      imageUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuBqXBTpHlCae4ZTVnazrAdFqPatrO28sSPDMJwLg6wHzFmr2q2MSEnRgiWZyCHwKnfZeMQuvct_Sp8eZUmSCFloNIlllblbPHJnirGD28rndGE3I73mbjoewYwrA4RqWPmEal9pX6kQ248IKJIfVr9eTAMjd78NbdBHafcoKgFt_LCNB9ZfsBAbS3VDNZL4wYnDWj1KRvGH_MrKi-dJqhKsUJ6ZtYdo10_4wZ1qcTKT4pC0FFji8D49L8-MI48bSWZDXBtuKoh0WmvN',
      members: [
        { name: 'User 4', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuB-3Of3WVHx0FtUE_mFSDViaoz1R-cS4nPzMNWP8fX_0BAkXKcNtOD3Z8Z5lvri3_GNmL-CdHfc-P-Px5w4wpq9pgXiuV-hcHfis3SRuJOaVKTfnAg-ON4VTgVftdVIw0ywK4ljyll5QQwNta7iAUpoIzINh9ElY3hTiqHGaQCyxj1UnKjW5J4Olr2hUYx8CaDE-fsSfyakRn-DDPRDVdpyRf5cH8nzGyxm6k1fjPDVJFvencsH-NnJroYaA_T5-G0V3OqAjz_FOBg0' },
        { name: 'User 5', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuAgKqLH2rAqeCVnICXuFl3TgEMDrei2Via8ZavvpFOVEC_hCBUQ9eUsm8QZ9bV-Va0rPp3XDa1EWggkJthCb-_S9YTeoHJ6DElSNeI55mK6qsU_ZrFF6NEy3y0yUBSYw1D41FD-YcuDzfIqblf676Y6mlqZCFzEAvaYmWH_m2A-Ag9V3S9dH6QrB1HAzIPEFuWsgicjYXKam_GA7i-CfxNynKXnNG0yVxNQHg_WLTlIABKZq9dAk8XY2d_DFPjBecYPcpVfdVgdtvjT' }
      ],
      moreMembersCount: 2,
      columns: [
        { id: 'todo', title: 'To Do', tasks: ['task-1', 'task-2'], color: 'white/40' },
        { id: 'inprogress', title: 'In Progress', tasks: ['task-3', 'task-4'], color: 'primary' },
        { id: 'done', title: 'Done', tasks: ['task-5', 'task-6'], color: 'green-400' }
      ],
      tasks: this.getTasks()
    },
    {
        id: 'design-system-updates',
        title: 'Design System Updates',
        lastModified: '1d ago',
        progress: 90,
        imageUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuBzN6esebG2oVLxNudLuIKLAywRYmrdpeYaSWU71JlEMxAInfOlosDjG4BqczHvz7nLQ7xKhEtOzPFDqKwrrmkrTOQEX4lyFEcXXQUqTr-CAxIEdEiZcTxfKTt9Y4Ujoi6Nhd-omKQzZszDWkCeJ9rCl1yNCs4_9_HWaXfpmaZsyHznSNdHnFDU3JLBpHUKRzK8joK1HsQFsiT9z5H0ZTpKkQlQNI71AGBJhFPiAH-mAvE_NVS6w3Se4d1UowHLjK5MNoSoF92X9bse',
        members: [
            { name: 'User 6', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuAfXJkj-4ivm4xoAYnxO-eeliKAktjBq25dUj34g_HrbXVeRLRtQ0i8iB5fTO3Kr3iQ_SAk9r4chDvm1I5Lhrv2aoAmlsOl1m9rStuZsR9fD3EtOuR12YKBZ2R2rIvbH99LTpbTmmYN-JSNO-S2xXwgkkw0f8rMMGPF6XdCFXPkrUdIvGMHwWOXt-Px9ndGSp-SrQhvqT-Y0XeSARnmnibuubzATDtzZ_iufKB0wLuJ1Tybbi9cJOSlhJ8oPsrpsww9wDR_6iiCwmuY' },
            { name: 'User 7', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuAn1P1_Dv2K7aRafTvW1uttY5A_br5RxgfVya-RVBSQRtg3ZpfX35weUuDJNOEt6P5mBtMtNvAN1DKifPTervWnzinW_gZFaDuy3quaXujmb-vOrU1gsMVVsoY_KeQMQtdgro2GdWIGGyhLulLXdkPj4FEQmMpiTT44faDfDHjkMVR6Tjsk1MlYN-QQ3Q1Jv2RG3b-778VuEjah8oCQbnlMW6NbVF2HDV1j-w_uXYMJPbKHfLC5nsnCM6Zod5VsWlXYd7WeWibGOEa_' }
        ],
        moreMembersCount: 0,
        columns: [],
        tasks: []
    },
    {
        id: 'client-onboarding',
        title: 'Client Onboarding',
        lastModified: '3d ago',
        progress: 15,
        imageUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuC0YlhH_-Zj0-pB_4H_V1MzGGryAaxAyZv63KhdTx7UDq6LmXZibcVErb7xLkc_8vYFErCzMOJkAqUl9Jbsk3htyUDc2mWIJakn_9uWhHiieCQMjK6gwGVVwUHsXWkVy0GhYQlexCz6CwKW4h1xg9XhVmGwujiz-vQY99fRYG3eRBCoEgkrzTgZfi1iXGtmdjOrP6I6lCnJaaZolpgvL3BItuXKL8Ko7wlcR8scfAmo8_GCwXk_tyOVRqEefAKdMfqy99-Drx8yYWHQ',
        members: [
            { name: 'User 8', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuAAMWG6Oa3jUyKBC07A-HO8XRewPmW1IRkD9X_E0AzL_lakhAk46FpZJYK2nmpuhuTPU9inAHXFxB2cLnTwbTPkjXaO-Lr-d4QEx3bYswM1FA8-1n2Fv1t2bFa3qcS0XOkcNAc7-n7W33pdBKOG8leYViPKT3UIh4zRACRDV_B403Z5WuaT5pQNmA-SQ8LuapwZatuIi2y3MFcs1IrAmh36g-HW1RvwP8lUO-1xCL6C3NXqn2cTcW-MCcvXA4iTRCkl1elA70jdnXDe' },
            { name: 'User 9', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuBpKuwf9o_NXvf_b5EOnpTS-z6Mne3qVhIdyr0PAWbZmVld2tdvQpDWAEGoCKGL70kyMO_ctZvsRoQxdvG8Bb-M-Rxo1D-5QgfJmfQP1wIWoYR23Sxd45XAUMHS2sPIfG3pTLOUX_fEcIxvo1zYem27iL9Kuwm-_pV6CMXhXntJJ2y5jkmVRRrIGrYWMP30ZvfpNe5vmUpqaCbIyfY-l5w8-7ewvqhK5YA9Zj41Glh7SLnWW-_JiGq1kjbt8I8tjKok8Dm3LX8US_78' },
            { name: 'User 10', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuDeada7AZW3z7mDgpmmnnKaMCOlCirTVNZ2aSom1H6ry4ZDhvWPuSbQCkZaD_B9ZfWCQVYvKDe6B0y5pStCRHXVUkJ-NgxUga4wGwLwYz4v-gDa6u-0QWvmmrCwh7wvsvXsTGleZnbrxMToDSYlRuVdpFljlvW-vD2P1PR2chfFrOtiH8m0bfj3HtSXvbsrB08yanFPU3UL-RbEUwiJPaol5MU7WMCFgaGx8Bpe7Sl1OBio_5tL9m5ooNeOIHHZLsj8GGN4W06bvSlQ' }
        ],
        moreMembersCount: 12,
        columns: [],
        tasks: []
    },
    {
        id: 'mobile-app-v2',
        title: 'Mobile App V2',
        lastModified: '1w ago',
        progress: 62,
        imageUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuAL5hxYNcSwLYviKqEGTwyE1EgTRzWWao7_QjmISYJseG9qQ6IZ2y_1MSUR-H7-g6nc6KZlrFTdC2VJ0ZjPw5Rnu9G7O3xk8DBpR6Wuj6qaYGvp6ryjgih_nMcL80fyMe8OiXn1CS4r6T3N2oMrqFGf45-eFwE6uwULFJyRbTED1LN9Q6N-FCcjzSwAkLB9-7JYO_UkQx1FO6fNZUlear6esp7qKYcsgiZrHOEbmsF9B564_JGt23-yexT0GV37UzykanEe2wQbL8H6',
        members: [
            { name: 'User 11', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuBUeRYBK7JBBmOa9JgYd-Ho0i325Q6WOj6KfobR6CXIVX_FqhfrlXXDkij3i12ajNJlG6ClH7Reiy6k2sXoBok4Bq3hQVCchzJKwDLMfb55cpQxiSopkBkaQHHoJY0nmPRJ7xMJIsNgFZTdY3da9az7XGg6hduhU4R5Vp-Nt4hWF4iZp8-N5gN3U911kqg5g9k2QO1zdPPWvjm11c70SCdSmpN1a0ifj5thaMEeR_ua2MShB4jjUIjCxn6sgBYXfGfQjTHo6hz6PlMt' },
            { name: 'User 12', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuD3iI1WUtDGUu7HMpRSOsRIEUzTbfNlCPVyQqT3YuvYAfOjK5H_dOtdVSJWapT7Y0mJ2BuShiiu9SMNTyVAK1XMABduY7DvB5j5gY47it4E25gxPGSwirOUO8UJhwJSKCyV72TJJ6rke3HLa7rj01IBnVpoPSL_zNAvXkDsvjPl-I4B74LsiIP58G2Hsyu4ogvUkc5FH_BKn8C_gLpHCtd-HunBL7i5egr19wKit6odKIJ9Z9He2eYzTbXDBVXo-MB2fsPDj3-Xe5A8' },
            { name: 'User 13', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuDTPavTnWSr9zyFwt42axEIyALG2AzGclUIQ3dRMzKygv_eazv5pCP6XpJUFgCYdOig8axmz__EnL-85eb4tjpeSLVR6IvZCLvh7oxf8qMU03iRczcUOsJvyJQ9RWsKyLcCuNo-qwpbPAqiW7uq-DA_LG1f44buur8-zaSK6t7vTFD7w6-r2H0iXlWHfYf7GhqndBGqcDURSsXC7lcwKUhO2Ol6ygD_etGDIFHy6tcc50dCGIDuK-4OFrvAs3rRW6TyEQ-Zi6TITE9S' }
        ],
        moreMembersCount: 0,
        columns: [],
        tasks: []
    }
  ]);
  
  getBoards() {
    return this.boards.asReadonly();
  }

  getBoard(id: string) {
    return this.boards().find(b => b.id === id);
  }

  getTask(board: Board, taskId: string) {
    return board.tasks.find(t => t.id === taskId);
  }

  private getTasks(): Task[] {
    return [
      { 
        id: 'task-1',
        title: 'Update Design System with new Glassmorphism variables',
        tags: [{ name: 'Design', color: 'bg-primary/30', textColor: 'text-primary' }],
        commentsCount: 4,
        attachmentsCount: 2,
        assignees: [{ name: 'User 1', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuDC7rfluPax3PUNIuy7GYSJBUiz0GxcCOXHl-1mvHSiGZmExNGKuM83kpQHcYOGQSNHHEbt2mDbVJbV7zxhPVmsBGaK9y9jpXZwSzk7ryf0xZjpACvK6JVYlXOHALBf3s7K2JvMVg7mZy6hKuMTT16jzTr9QGj0P4huRmDHz7dWCQzTSMFY588-mD09R9btEBVq-6R3LphgEs08b4AkT6D55Z4zLG1CyYPFaFjM-m5gA-ZI1NLH8SGW7U86C3lph6MuLjYsDrZA1suK' }],
        completed: false,
        status: 'todo',
        description: 'The objective is to modernize the landing page with a heavy emphasis on visual storytelling. We need to implement the new glassmorphism design system across all primary components. Focus on high contrast, readability, and performance.',
        checklist: [
          { item: 'Draft visual hero concept', completed: true },
          { item: 'Select typography and color palette', completed: true },
          { item: 'Implement glassmorphism CSS utilities', completed: false },
          { item: 'Final mobile responsiveness review', completed: false },
        ]
      },
      { 
        id: 'task-2',
        title: 'Integration of the Stripe Connect flow for vendor onboarding',
        tags: [{ name: 'API', color: 'bg-orange-500/20', textColor: 'text-orange-400' }],
        dueDate: 'Sep 24',
        commentsCount: 0,
        attachmentsCount: 0,
        assignees: [{ name: 'User 2', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuDsnP1_qcdEjUrxCz5_5qyZ3uVD_72F6MRrgL2qX0V-U-L6uk9i_etJDhmNjcg0oy6VG-0-6sR38DH6PCQsxc8F495Zy9LplRESjv4eMj3nwaTYI7sHgfEgri1ZClVdUTIQGv_eVJJI_Nr6_xkBBLUVDCBY5ad1LiO4wm3GN_CGG2DZSyqh-HU642Rk2zNhXZzLmxIzTO2baNtaD7sM_WMnIDZFD3drGju6T83bZ3gL9BRE5AYeVr8TSYkDBICZuBq433SbXsIG6YMy' }],
        completed: false,
        status: 'todo',
        description: '',
        checklist: []
      },
      { 
        id: 'task-3',
        title: 'Refining the Glassmorphism UI Kit components and examples',
        tags: [{ name: 'UI Kit', color: 'bg-blue-400/20', textColor: 'text-blue-300' }],
        progress: 66,
        commentsCount: 12,
        attachmentsCount: 0,
        assignees: [
            { name: 'User 3', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuCytFjZDLveIA_-W0TiNvhweqBuLDDUDAddQOb_S_Q8FiNRG8dVNHHksmAY3sgJTlCU2epL5r7G_m614CDb9pk02LOFfa6LeWwl2yWuy3qO-wp9lstXDoSXqYSmWs5chXrs3X4SLSUQ6jAmc5nO6B-OW2XNED_Z384aWCPPM1GCLqOWePjWTRQTr3U-xHDyiFK-Mt0UqwhJto7iZlQxDyU2arteCdWTpmDbElwDBden8G9x-JaO1GYu1R_oVsEXVbu85CDDWKsSQ00v' },
            { name: 'User 4', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuDlXGu1yoDuvYMjIJvoel6CC4U7zkO51Iytm-Zs775kN-K4NtwbJb3e1EIOzqbVqc2nJL_sF1Y0m0JR31zZ_ZTwn7UQTMignHDogY5cx0jYuzZPkbagJaA9fsD0Dt88ddcXaQcN13JvKG6nAst3P2vHOOR6s_c--S3Eb9967fs2iBTD1gD6R3mQk4uug9NoZHduP6zX_mPVQQ1NP47cOAPAlWurqOovcJrNG3NzKkaH9UabUQOG5V2Wu69RCPcGXuHk_7cs0l-2c3Gr' }
        ],
        completed: false,
        status: 'inprogress',
        description: '',
        checklist: []
      },
      { 
        id: 'task-4',
        title: 'User testing session with beta group for the new workspace layout',
        tags: [{ name: 'Research', color: 'bg-purple-400/20', textColor: 'text-purple-300' }],
        commentsCount: 0,
        attachmentsCount: 1,
        assignees: [{ name: 'User 5', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuD3R0P1RkMOLSOhxG8LvB4B50-U_HhNf9DzVWyUrCu_cAmUDBMmnqJwLef64MN1rrKdOFEQneZz4Qi8sENXhwqoeA8KBtVlpY4NRttStpkAjncOD2sQIY9lZt5nreL-CeC8I5DlxQnwdeizikiEib4RK03NcWC9iduIPKv4wII24m6pwXEX76yWrV-ZFSzPfVGsUCZ25b0ttIxl1EZLodGeV2AVHDRb6ggVtaZGeVnkIZokvqcFQD-a3PB2ZJTnRnYbYckEjIVJFzz8' }],
        completed: false,
        status: 'inprogress',
        description: '',
        checklist: []
      },
      { 
        id: 'task-5',
        title: 'Q4 Product Kickoff and OKR setting session',
        tags: [{ name: 'Strategy', color: 'bg-green-400/20', textColor: 'text-green-400' }],
        commentsCount: 0,
        attachmentsCount: 0,
        assignees: [{ name: 'User 6', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuAw4g3KDNoa0KQgr8mfaoY2BvfVrWKKvCIv8wR18-NIIbR4cwMhanOYCOURCH7oslTRToKgptt5TFgjAPhVK0Z8RFyLJ5VgzZk5DiltopBQuuzAEbnMDsWMH8_DudVr0HRR4rXBL3_JqAp4VCmf1EA0OVPp23wXEnSxTOqYVFSPP30Ipnfqm_iPts35URilHRMkrA4cfNa47dajd6DJ021lSMJ4kM3L_8Im4OFY0eQG8fxF-35OA2t4zv7ZDacsZ3J0qvHRSVgbF9LJ' }],
        completed: true,
        status: 'done',
        description: '',
        checklist: []
      },
      { 
        id: 'task-6',
        title: 'Client approval for the updated privacy policy and terms',
        tags: [{ name: 'Legal', color: 'bg-green-400/20', textColor: 'text-green-400' }],
        commentsCount: 0,
        attachmentsCount: 0,
        assignees: [{ name: 'User 7', avatarUrl: 'https://lh3.googleusercontent.com/aida-public/AB6AXuDg9Ybg8rEDZvM_l85Zno8JrcBLO_7y7WOY3XnQHRbjZCiRTmN73KQzr1g1b4mFnMxZvnfZmIY56xO3CChDIPXb2iybgkoVBZArpLLBN6PLn2EcjpJcYjJTGNlnkj03S41I5-YmnM6B_2VkA_K839IV1ZrDusYMpmBNwOPm_GLiz8ICVy3EF3qDV2HVu8bQF8aKIucCHiOF9OAxPvpfLPhr9MAZtK8x4Ng_HtJwRybIlj4JQx1kuzbGwYgiVR52Lyffu2stPGZYjaO8' }],
        completed: true,
        status: 'done',
        description: '',
        checklist: []
      },
    ];
  }
}
