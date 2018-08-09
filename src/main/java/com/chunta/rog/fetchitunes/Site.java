package com.chunta.rog.fetchitunes;

public class Site {
	private String picture,nameArtist,nameSong,rank;
	
	public Site(){ }
	public Site( String picture, String nameArtist, String nameSong, String rank){
		
		this.picture = picture;
		this.nameArtist = nameArtist;
		this.nameSong = nameSong;
		this.rank = rank;
	}
	
	public void setPicture(String picture) {
		this.picture = picture;
	}
	public String getPicture() {
		return picture;
	}
	public void setNameArtist(String nameArtist) {
		this.nameArtist = nameArtist;
	}
	public String getNameArtist() {
		return nameArtist;
	}
	public void setNameSong(String nameSong){
		this.nameSong = nameSong;
	}
	public String getNameSong(){
		return nameSong;
	}

	public void setRank(String rank){
		this.rank = rank;
	}
	public String getRank(){
		return rank;
	}
}
