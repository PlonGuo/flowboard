
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-whiteboard',
  template: `
<div class="bg-background-dark h-screen w-screen relative overflow-hidden">
    <!-- Main Canvas -->
    <div class="absolute inset-0 canvas-grid opacity-40"></div>
    
    <!-- UI Layer -->
    <div class="relative z-10 flex flex-col h-full w-full pointer-events-none">
        <!-- Header -->
        <header class="flex items-center justify-between px-8 py-6 pointer-events-auto">
            <div [routerLink]="['/']" class="flex items-center gap-3 glass px-4 py-2 rounded-full cursor-pointer">
                <div class="text-primary"><span class="material-symbols-outlined text-3xl">hub</span></div>
                <h1 class="text-lg font-bold tracking-tight">FlowBoard</h1>
                <div class="h-4 w-[1px] bg-white/20 mx-1"></div>
                <span class="text-sm text-white/60 font-medium">Q3 Product Strategy</span>
            </div>
            <div class="flex items-center gap-4">
                <div class="glass flex items-center px-2 py-1.5 rounded-full">
                    <div class="flex items-center -space-x-2 mr-3">
                        <div class="size-8 rounded-full border-2 border-background-panel bg-cover bg-center" style="background-image: url('https://lh3.googleusercontent.com/aida-public/AB6AXuAvpvYzcUc8VAjyQpbWvxnb1uX5Qw4imCm6FvFlxZ8VNJRZjwVQZiFy70esW2YNFuL8T9P-I2Yo1U60uIh9LbQupF2pi3K2cXdc5uhWiuHiOSew4E_8PTTcgbZSOoW9nS9HLSVDRUx9TQNf_zzKj2VlM6teC9aGPUOoD-sTonzrqiNolpmuIu4yZ40U12n1ButTwiBsg9qyuRFio-V6bEXONBq2RMG6F-INiCWW1LeIrLp60eBk0mvYJlx3UrDoXarF4rWwHNRVuvVS')"></div>
                        <div class="size-8 rounded-full border-2 border-background-panel bg-cover bg-center" style="background-image: url('https://lh3.googleusercontent.com/aida-public/AB6AXuCB0I07Xi2HoJjCmG-suK2drEySfblk7ZjjA8iQ9ueVdn5cT0XEWDqkm0iKBLqaGz-TOWJJeBh1gTVp-O0EtAxD8qpAaafYO2i4FclA0oh6kdzqZl0H6NMRugnFBCYf9Z5V1Hyad-UR_ZOI4bclrWgO3cRzeiLLbIoR4-iY8kFq2IDfQEZpJN2SDifc1gqCtfMsX5eYtBaiZCSOs5wAo8dYn0bFVXGnhFcsGYf-jaIjPkJgPVNYxKquoqzHEAYve5a7rBYl9sLchcYG')"></div>
                        <div class="size-8 rounded-full border-2 border-background-panel bg-primary flex items-center justify-center text-[10px] font-bold">+5</div>
                    </div>
                    <button class="bg-primary hover:bg-primary/90 transition-colors px-5 py-2 rounded-full text-sm font-bold flex items-center gap-2"><span class="material-symbols-outlined text-lg">share</span>Share</button>
                </div>
                <button class="glass size-11 flex items-center justify-center rounded-full"><span class="material-symbols-outlined text-xl">settings</span></button>
            </div>
        </header>
        
        <!-- Floating Toolbar -->
        <div class="absolute top-24 left-1/2 -translate-x-1/2 pointer-events-auto">
            <nav class="glass p-2 rounded-full flex items-center gap-1">
                <button class="size-12 flex items-center justify-center rounded-full text-white/50 hover:text-white hover:bg-white/5 transition-all"><span class="material-symbols-outlined">near_me</span></button>
                <button class="size-12 flex items-center justify-center rounded-full bg-primary text-white shadow-lg shadow-primary/20"><span class="material-symbols-outlined">edit</span></button>
                <button class="size-12 flex items-center justify-center rounded-full text-white/50 hover:text-white hover:bg-white/5 transition-all"><span class="material-symbols-outlined">rectangle</span></button>
                <button class="size-12 flex items-center justify-center rounded-full text-white/50 hover:text-white hover:bg-white/5 transition-all"><span class="material-symbols-outlined">image</span></button>
                <button class="size-12 flex items-center justify-center rounded-full text-white/50 hover:text-white hover:bg-white/5 transition-all"><span class="material-symbols-outlined">text_fields</span></button>
                <button class="size-12 flex items-center justify-center rounded-full text-white/50 hover:text-white hover:bg-white/5 transition-all"><span class="material-symbols-outlined">link</span></button>
            </nav>
        </div>

        <!-- Canvas Content -->
        <div class="flex-1 flex items-center justify-center relative">
            <svg class="absolute w-full h-full" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M calc(50% - 130px) 50% C calc(50% - 30px) 50%, calc(50% + 30px) calc(50% + 10px), calc(50% + 130px) calc(50% + 10px)" stroke="#6467f2" stroke-width="2" stroke-dasharray="4 4" class="opacity-50"></path>
            </svg>
            <div class="absolute top-1/2 left-1/2 -translate-x-[240px] -translate-y-[50%]">
                <div class="glass w-56 p-6 rounded-xl border-l-4 border-l-primary flex flex-col justify-between">
                    <p class="text-white/80 text-sm leading-relaxed">Implement real-time presence indicators with glass tags.</p>
                </div>
            </div>
            <div class="absolute top-1/2 left-1/2 translate-x-[80px] -translate-y-[calc(50%-10px)]">
                <div class="glass w-56 p-6 rounded-xl border-l-4 border-l-pink-500 flex flex-col justify-between" style="background: rgba(236, 72, 153, 0.05);">
                    <p class="text-white/80 text-sm leading-relaxed">Research glassmorphism accessibility standards.</p>
                </div>
            </div>
        </div>
        
        <!-- Footer -->
        <footer class="flex items-end justify-between px-8 py-8 pointer-events-auto">
             <div class="glass p-2 rounded-xl"><div class="w-32 h-20 bg-background-dark/50 rounded-lg relative overflow-hidden"><div class="absolute inset-0 canvas-grid opacity-20"></div><div class="absolute inset-4 border border-primary/50 bg-primary/5 rounded"></div></div></div>
            <div class="glass px-4 py-1.5 rounded-full flex items-center gap-2"><div class="size-2 rounded-full bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.8)]"></div><span class="text-[10px] font-bold uppercase tracking-widest text-white/60">Live Syncing</span></div>
            <div class="glass flex items-center p-1 rounded-full">
                <button class="size-9 flex items-center justify-center rounded-full text-white/60 hover:text-white hover:bg-white/5"><span class="material-symbols-outlined">remove</span></button>
                <div class="px-3 text-xs font-bold w-14 text-center">92%</div>
                <button class="size-9 flex items-center justify-center rounded-full text-white/60 hover:text-white hover:bg-white/5"><span class="material-symbols-outlined">add</span></button>
            </div>
        </footer>
    </div>
</div>
  `,
  styles: [`
.glass {
    background: rgba(255, 255, 255, 0.03);
    backdrop-filter: blur(12px);
    -webkit-backdrop-filter: blur(12px);
    border: 1px solid rgba(255, 255, 255, 0.08);
    box-shadow: 0 8px 32px 0 rgba(0, 0, 0, 0.37);
}
.canvas-grid {
    background-image: radial-gradient(rgba(255, 255, 255, 0.1) 1px, transparent 1px);
    background-size: 32px 32px;
}
:host {
    --background-dark: #0a0a14;
    --background-panel: #0a0a14;
}
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterModule]
})
export class WhiteboardComponent {
}
