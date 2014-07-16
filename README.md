 Breve explicação sobre o algorítimo de descompressão:
===========================
 
#### HEADER
	O primeiro byte é 0x46, cuja função é desconhecida.
	O segundo byte representa a quantidade de blocos de compressão. Nesse caso, 3.
	A partir desse ponto os bytes são lidos em grupos de 4, em Big Endian. (Int32)
	São três grupos para cada arquivo:
		Os quatro primeiros bytes apontam para o offset onde começa o bloco de compressão, é um ponteiro relativo, portanto ele deve ser somado com o offset do inicio do header.
		Os próximos quatro bytes representam o tamanho do arquivo após ser descomprimido.
		Os últimos quatro bytes representam o tamanho do bloco comprimido.
	E assim continua para cada arquivo.
   
---
#### COMPRESSÃO
	A compressão funciona da segunte forma:
	Começa com um byte que indica se os próximos 8 bytes estão ou não comprimidos.
	Cada bit desse byte é lido da direita para esquerda. 
	Cada bit 0 indica um valor comprimido(um par de bytes) e cada bit 1 indica um valor descomprimido(um único byte).
	O byte descomprimido pode ser copiado para o arquivo descompimido(sendo este um array ou uma string).
	O par de bytes é lido da seguinte forma:
		O primeiro byte deve ser somado a 0x12 e ao primeiro nibble do segundo byte. Este valor indica o offset, partindo do inicio do arquivo descomprimido, onde os bytes serão copiados. 
		O segundo byte é dividido em dois nibbles. (Um nibble contém 4 bits).
			Da esquerda pra direita, o primeiro nibble deve ser multiplicado por 0x100 e então somado ao primeiro byte.
			O segundo nibble somado a 3 indica quantos bytes serão copiados a partir do offset conseguido anteriormente.
	Ele usa uma janela de buffer de 0x1000(não tenho absoluta certeza, mas funciona). Quando o tamanho do arquvivo descomprimido for maior ou igual a 0x1000, esse mesmo valor deve ser somado ao offset que indica de onde os bytes serão copiados, mas se e apenas se o offset + 0x1000 for menor que o tamanho do arquivo descomprimido.