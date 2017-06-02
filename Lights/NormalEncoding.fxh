#ifndef NORMALENCODING_FXH
#define NORMALENCODING_FXH

//reference: http://aras-p.info/texts/CompactNormalStorage.html
 /*
//stereographic projection
half2 encodeNormals(half3 n)
{
	if(n.z == -1){
		n.z = -1.01;
	}
	half scale = 1.7777;
    half2 enc = n.xy / (n.z+1);
    enc /= scale;
    enc = enc*0.5+0.5;
    return half4(enc,0,0);
}

half3 decodeNormals(half2 enc)
{
    half scale = 1.7777;
    half3 nn =
        half3(enc.xy, 0)*half3(2*scale,2*scale, 0) +
        half3(-scale,-scale, 1);
    half g = 2.0 / dot(nn.xyz,nn.xyz);
    half3 n;
    n.xy = g*nn.xy;
    n.z = g-1;
    return n;
}*/

/*
//stereographic projection couldn't handle vectors of {0,0,-1}, looking like a bunch of z-fighting
//so we're just storing x&y, reconstruct z
half2 encodeNormals(half3 n)
{
	return half4(n.xy*0.5+0.5,0,0);
}


half3 decodeNormals(half2 enc)
{
	half3 n;
	n.xy = enc*2-1;
	n.z = sqrt(1-dot(n.xy, n.xy));
	return n;
}
*/

//jk, method #1 and #7 were garbage, spheremap transform is the way to go
half2 encodeNormals(half3 n)
{
	half p = sqrt(n.z*8+8);
    return half4(n.xy/p + 0.5,0,0);
}


half3 decodeNormals(half2 enc)
{
    half2 fenc = enc*4-2;
    half f = dot(fenc,fenc);
    half g = sqrt(1-f/4);
    half3 n;
    n.xy = fenc*g;
    n.z = 1-f/2;
    return n;
}

#endif