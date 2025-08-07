public HookedASM_rglAnimTree_Tick

EXTERN HookedWithParams_rglAnimTree_Tick:PROC
EXTERN Original_rglAnimTree_Tick:QWORD

.CODE
HookedASM_rglAnimTree_Tick	PROC
		
		;Saving these registers because they are used in HookedWithParams_rglAnimTree_Tick
		;Other volative registers (R9, R10, R11, XMM0-5) are set again the native function's loop 
		;so it's not important to save them
		;Other registers that need to be preserved are R12, R15, RDI, XMM11, XMM13, but these are callee-saved anyway

		;R11 holds the current bone buffer matrix frame, R10 is current -1, R9 is -2, and RCX is -3

		push	rcx
		push	rax
		push	rdx
		push	r8
		
		;Allocating stack for shadow-space (32 bytes), and a local array holding 4 pointers (32 bytes) for HookedWithParams_rglAnimTree_Tick
		sub		rsp, 64
	IFDEF EDITOR
		mov		[rsp + 64], r11
		mov		[rsp + 56], r10
		mov		[rsp + 48], r9
		mov		[rsp + 40], rcx
		lea		r8, [rsp+40]
		mov		dl, al
		mov		rcx, rdi
	ELSE
		mov		[rsp + 64], r11
		mov		[rsp + 56], r10
		mov		[rsp + 48], r9
		mov		[rsp + 40], rcx
		lea		r8, [rsp+40]
		mov		dl, r13b
		mov		rcx, rdi
	ENDIF
		call	HookedWithParams_rglAnimTree_Tick
		add		rsp, 64

		pop		r8
		pop		rdx
		pop		rax
		pop		rcx
	IFDEF EDITOR
		cmp		al, r15b
	ELSE
		cmp		r13b, r14b
	ENDIF
		jmp		Original_rglAnimTree_Tick
HookedASM_rglAnimTree_Tick	ENDP

.CODE
END