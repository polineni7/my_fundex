import { create } from 'zustand';
export const useAuth=create(set=>({user:JSON.parse(localStorage.getItem('user')||'null'),login:(accessToken,user)=>{localStorage.setItem('token',accessToken);localStorage.setItem('user',JSON.stringify(user));set({user});},logout:()=>{localStorage.clear();set({user:null});}}));
